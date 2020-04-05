/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using FiftyOne.Pipeline.JavaScriptBuilder.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.Data;

namespace FiftyOne.Pipeline.JavaScriptBuilder.FlowElement
{
    /// <summary>
    /// Builder for the <see cref="JavaScriptBuilderElement"/>
    /// </summary>
    public class JavaScriptBuilderElementBuilder
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger _logger;

        protected string _host = string.Empty;
        protected bool _overrideHost;
        protected string _endpoint = string.Empty;
        protected string _protocol = string.Empty;
        protected bool _overrideProtocol;
        protected string _objName = string.Empty;
        protected bool _enableCookies;
        protected bool _minify = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="loggerFactory">
        /// The logger factory.
        /// </param>
        public JavaScriptBuilderElementBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<JavaScriptBuilderElementBuilder>();
        }

        /// <summary>
        /// Set whether the client JavaScript stored results of client side
        /// processing in cookies.
        /// </summary>
        /// <param name="enableCookies">Should enable cookies?</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetEnableCookies(bool enableCookies)
        {
            _enableCookies = enableCookies;
            return this;
        }

        /// <summary>
        /// Set the host that the client JavaScript should query for updates.
        /// </summary>
        /// <param name="host">The host name.</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetHost(string host)
        {
            _host = host;
            return this;
        }

        /// <summary>
        /// Set whether host should be determined from the origin or referer.
        /// </summary>
        /// <param name="overrideHost">Should override host?</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetOverrideHost(bool overrideHost)
        {
            _overrideHost = overrideHost;
            return this;
        }

        /// <summary>
        /// Set the endpoint which will be queried on the host. e.g /api/v4/json
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        /// <summary>
        /// The default protocol that the client JavaScript will use when 
        /// querying for updates.
        /// </summary>
        /// <param name="protocol">The protocol to use (http / https)</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetDefaultProtocol(string protocol)
        {

            var empty = string.IsNullOrEmpty(protocol);
            var http = string.Equals(protocol, "http");
            var https = string.Equals(protocol, "https");

            if (string.Equals(protocol, "http") ||
                string.Equals(protocol, "https"))
            {
                _protocol = protocol;
            }
            else
            {
                _protocol = Constants.DEFAULT_PROTOCOL;
                _logger.LogWarning($"No/Invalid protocol in configuration," +
                    $" JavaScriptBuilderElement is using the default protocol:" +
                    $" {Constants.DEFAULT_PROTOCOL}");
            }
            return this;
        }

        /// <summary>
        /// Set whether the host should be overridden by evidence, e.g when the
        /// host can be determined from the incoming request.
        /// </summary>
        /// <param name="overrideProto">Should override the protocol?</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetOverrideDefaultProtocol(bool overrideProto){
            _overrideProtocol = overrideProto;
            return this;
        }

        /// <summary>
        /// The default name of the object instantiated by the client 
        /// JavaScript.
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetObjectName(string objName)
        {
            var match = Regex.Match(objName, @"[a-zA-Z_$][0-9a-zA-Z_$]*");

            if (match.Value == objName)
            {
                _objName = objName;
            }
            else
            {
                var ex = new PipelineConfigurationException("JavaScriptBuilder" +
                    " ObjectName is invalid. This must be a valid JavaScript" +
                    " type identifier.");

                _logger.LogCritical(ex, "Value for ObjectName is invalid.");
                throw ex;
            }

            return this;
        }

        public JavaScriptBuilderElementBuilder SetMinify(bool minify)
        {
            _minify = minify;
            return this;
        }

        /// <summary>
        /// Build the element.
        /// </summary>
        /// <returns></returns>
        public virtual IFlowElement Build()
        {
            return new JavaScriptBuilderElement(_loggerFactory.CreateLogger<JavaScriptBuilderElement>(),
                CreateData,
                _host,
                _overrideHost,
                _endpoint,
                _protocol,
                _overrideProtocol,
                _objName,
                _enableCookies,
                _minify);
        }

        private IJavaScriptBuilderElementData CreateData(
            IPipeline pipeline,
            FlowElementBase<IJavaScriptBuilderElementData, IElementPropertyMetaData> 
                javaScriptBuilderElement)
        {
            return new JavaScriptBuilderElementData(
                _loggerFactory.CreateLogger<JavaScriptBuilderElementData>(),
                pipeline);
        }
    }
}
