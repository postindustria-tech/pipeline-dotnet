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

using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using FiftyOne.Pipeline.JsonBuilder.FlowElement;
using FiftyOne.Pipeline.Web.Framework.Configuration;
using FiftyOne.Pipeline.Web.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiftyOne.Pipeline.Web.Framework
{
    /// <summary>
    /// Singleton class which contains an IPipeline build from a configuration
    /// file, and some extra options which are available in a web environment.
    /// </summary>
    public class WebPipeline
    {
        private static WebPipeline _instance = null;

        /// <summary>
        /// Whether or not client-side properties are enabled. If they are,
        /// then a 51Degrees.core.js will be served by the server.
        /// </summary>
        public bool ClientSideEvidenceEnabled => _options.ClientSideEvidenceEnabled;

        /// <summary>
        /// Extra pipeline options which only apply to an implementation in a
        /// web server.
        /// </summary>
        private readonly PipelineWebIntegrationOptions _options;

        /// <summary>
        /// Lock used when constructing a pipeline.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// The single IPipeline instance for a web server.
        /// </summary>
        public IPipeline Pipeline { get; private set; }

        /// <summary>
        /// Get the only instance of WebPipeline. If an instance does not
        /// already exist, one is created.
        /// </summary>
        /// <returns>
        /// The single instance of WebPipeline
        /// </returns>
        public static WebPipeline GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new WebPipeline();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Private constructor used to construct the only instance which will
        /// exist of this class.
        /// </summary>
        private WebPipeline()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddPipelineConfig()
                .Build();
            PipelineWebIntegrationOptions options = new PipelineWebIntegrationOptions();
            config.Bind("PipelineOptions", options);

            if (options == null ||
                options.Elements == null)
            {
                throw new PipelineConfigurationException(
                   Messages.ExceptionNoConfiguration);
            }

            // Add the sequence element.
            var sequenceConfig = options.Elements.Where(e =>
                e.BuilderName.IndexOf(nameof(SequenceElement),
                    StringComparison.OrdinalIgnoreCase) >= 0);
            if (sequenceConfig.Any() == false)
            {
                // The sequence element is not included so add it.
                options.Elements.Add(new ElementOptions()
                {
                    BuilderName = nameof(SequenceElement)
                });
            }

            if (ClientSideEvidenceEnabled)
            {
                // Client-side evidence is enabled so make sure the 
                // JsonBuilderElement and JavaScriptBundlerElement has been 
                // included.
                var jsonConfig = options.Elements.Where(e =>
                    e.BuilderName.IndexOf(nameof(JsonBuilderElement),
                        StringComparison.OrdinalIgnoreCase) >= 0);
                if (jsonConfig.Any() == false)
                {
                    // The json builder is not included so add it.
                    options.Elements.Add(new ElementOptions()
                    {
                        BuilderName = nameof(JsonBuilderElement)
                    });
                }

                var builderConfig = options.Elements.Where(e =>
                    e.BuilderName.IndexOf(nameof(JavaScriptBuilderElement), 
                        StringComparison.OrdinalIgnoreCase) >= 0);
                if (builderConfig.Any() == false)
                {
                    // The bundler is not included so add it.
                    options.Elements.Add(new ElementOptions()
                    {
                        BuilderName = nameof(JavaScriptBuilderElement)
                    });
                }
            }

            Pipeline = new PipelineBuilder(new LoggerFactory())
                .BuildFromConfiguration(options);

            _options = options;
        }


        /// <summary>
        /// Populate a FlowData's evidence from the web request, and process
        /// using the Pipeline.
        /// </summary>
        /// <param name="request">
        /// Request containing the evidence to process
        /// </param>
        /// <returns>
        /// A processed FlowData
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null.
        /// </exception>
        public static IFlowData Process(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Create a new FlowData instance.
            var flowData = GetInstance().Pipeline.CreateFlowData();
            // Add headers
            foreach (var headerName in request.Headers.AllKeys)
            {
                CheckAndAdd(flowData, "header." + headerName, request.Headers[headerName]);
            }
            // Add cookies
            foreach (var cookieName in request.Cookies.AllKeys)
            {
                CheckAndAdd(flowData, "cookie." + cookieName, request.Cookies[cookieName].Value);
            }
            // Add query parameters
            foreach (var paramName in request.QueryString.AllKeys)
            {
                CheckAndAdd(flowData, "query." + paramName, request.QueryString[paramName]);
            }
            if (request.RequestContext.HttpContext.Session != null)
            {
                CheckAndAdd(flowData, "session", new AspFrameworkSession(request.RequestContext.HttpContext.Session));
                foreach (var sessionValueName in request.RequestContext.HttpContext.Session.Keys)
                {
                    CheckAndAdd(flowData, (string)sessionValueName, request.RequestContext.HttpContext.Session[(string)sessionValueName]);
                }
            }
            // Add the client IP
            CheckAndAdd(flowData, "server.client-ip", request.UserHostAddress);
            // Process the evidence and return the result
            flowData.Process();
            return flowData;
        }

        /// <summary>
        /// Check if the given key is needed by the given flowdata.
        /// If it is then add it as evidence.
        /// </summary>
        /// <param name="flowData">
        /// The <see cref="IFlowData"/> to add the evidence to.
        /// </param>
        /// <param name="key">
        /// The evidence key
        /// </param>
        /// <param name="value">
        /// The evidence value
        /// </param>
        private static void CheckAndAdd(IFlowData flowData, string key, object value)
        {
            if (flowData.EvidenceKeyFilter.Include(key))
            {
                flowData.AddEvidence(key, value);
            }
        }
    }
}
