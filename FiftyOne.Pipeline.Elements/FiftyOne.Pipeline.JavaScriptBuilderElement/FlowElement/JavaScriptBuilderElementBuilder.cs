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
        protected string _endpoint = string.Empty;
        protected string _protocol = string.Empty;
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
        /// By default, the host from the request will be used.
        /// </summary>
        /// <param name="host">The host name.</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetHost(string host)
        {
            _host = host;
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
        /// The protocol that the client JavaScript will use when 
        /// querying for updates.
        /// By default, the protocol from the request will be used.
        /// </summary>
        /// <param name="protocol">The protocol to use (http / https)</param>
        /// <returns></returns>
        public JavaScriptBuilderElementBuilder SetProtocol(string protocol)
        {
            if (string.Equals(protocol, "http", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(protocol, "https", StringComparison.OrdinalIgnoreCase))
            {
                _protocol = protocol;
            }
            else
            {
                throw new PipelineConfigurationException(
                    $"Invalid protocol in configuration ({protocol}), " +
                    $"must be 'http' or https'");
            }
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
        public virtual JavaScriptBuilderElement Build()
        {
            return new JavaScriptBuilderElement(_loggerFactory.CreateLogger<JavaScriptBuilderElement>(),
                CreateData,
                _endpoint,
                _objName,
                _enableCookies,
                _minify, 
                _host, 
                _protocol);
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
