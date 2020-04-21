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

using System;

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using Microsoft.Extensions.Primitives;
using FiftyOne.Pipeline.Core.FlowElements;
using System.Linq;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;

namespace FiftyOne.Pipeline.Web.Services
{
    /// <summary>
    /// Class that provides functionality for the 'Client side Overrides'
    /// feature.
    /// Client side overrides allow JavaScript running on the client device
    /// to provide additional evidence in the form of cookies or query string 
    /// parameters to the pipeline on subsequent requests.
    /// This enables more detailed information to be supplied to the 
    /// application. (e.g. iPhone model for device detection)
    /// </summary>
    public class ClientsidePropertyService : IClientsidePropertyService
    {
        /// <summary>
        /// Character used by profile override logic to separate profile IDs 
        /// </summary>
        protected const char PROFILE_OVERRIDES_SPLITTER = '|';

        /// <summary>
        /// Device provider
        /// </summary>
        private IFlowDataProvider _flowDataProvider;

        /// <summary>
        /// Pipeline
        /// </summary>
        private IPipeline _pipeline;

        /// <summary>
        /// A list of all the HTTP headers that are requested evidence
        /// for elements that populate JavaScript properties 
        /// </summary>
        private StringValues _headersAffectingJavaScript;

        /// <summary>
        /// The cache control values that will be set for the JavaScript
        /// </summary>
        private StringValues _cacheControl = new StringValues(
            new string[] {
                "only-if-cached",
                "max-age=600",
            });

        /// <summary>
        /// Create a new ClientsidePropertyService
        /// </summary>
        /// <param name="flowDataProvider"></param>
        /// <param name="pipeline"></param>
        public ClientsidePropertyService(
            IFlowDataProvider flowDataProvider,
            IPipeline pipeline)
        {
            _flowDataProvider = flowDataProvider;
            _pipeline = pipeline;

            var headersAffectingJavaScript = new List<string>();
            // Get evidence filters for all elements that have
            // JavaScript properties.
            var filters = _pipeline.FlowElements
                .Where(e => e.Properties.Any(p => 
                    p.Type != null && 
                    p.Type == typeof(JavaScript)))
                .Select(e => e.EvidenceKeyFilter);
            foreach (var filter in filters)
            {
                // If the filter is a white list or derived type then
                // get all HTTP header evidence keys from white list
                // and add them to the headers that could affect the 
                // generated JavaScript.
                var whitelist = filter as EvidenceKeyFilterWhitelist;
                if (whitelist != null)
                {
                    headersAffectingJavaScript.AddRange(whitelist.Whitelist
                        .Where(entry => entry.Key.StartsWith(Core.Constants.EVIDENCE_HTTPHEADER_PREFIX + Core.Constants.EVIDENCE_SEPERATOR))
                        .Select(entry => entry.Key.Substring(entry.Key.IndexOf(Core.Constants.EVIDENCE_SEPERATOR) + 1)));
                }
            }
            _headersAffectingJavaScript = new StringValues(headersAffectingJavaScript.ToArray());
        }

        /// <summary>
        /// Add the JavaScript from the flow data object to the HttpResponse
        /// </summary>
        /// <param name="context">
        /// The HttpContext containing the HttpResponse to add the 
        /// JavaScript to.
        /// </param>
        public void ServeJavascript(HttpContext context)
        {
            // Get the hash code.
            var flowData = _flowDataProvider.GetFlowData();
            // TODO: Should this use a Guid version of the hash code to
            // allow a larger key space or is int sufficient?
            var hash = flowData.GenerateKey(_pipeline.EvidenceKeyFilter).GetHashCode();

            if (hash == context.Request.Headers["If-None-Match"])
            {
                // The response hasn't changed so respond with a 304.
                context.Response.StatusCode = 304;
            }
            else
            {
                // Otherwise, return the minified script to the client.
                var builder = flowData.Pipeline.GetElement<JavaScriptBuilderElement>();
                if(builder == null)
                {
                    throw new PipelineConfigurationException("Client-side " +
                        "JavaScript has been requested from the Pipeline. " +
                        "However, the JavaScriptBundlerElement is not present. " +
                        "To resolve this error, either disable client-side " +
                        "evidence or ensure the JavaScriptBundlerElement is " +
                        "added to your Pipeline.");
                }
                var builderData = flowData.GetFromElement(builder);

                SetHeaders(context, hash.ToString(), builderData.JavaScript.Length);

                context.Response.WriteAsync(builderData.JavaScript);
            }

        }

        /// <summary>
        /// Set various HTTP headers on the JavaScript response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hash"></param>
        /// <param name="contentLength"></param>
        private void SetHeaders(HttpContext context, string hash, int contentLength)
        {
            context.Response.ContentType = "application/x-javascript";
            context.Response.ContentLength = contentLength;
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("Cache-Control", _cacheControl);
            context.Response.Headers.Add("Vary", _headersAffectingJavaScript);
            context.Response.Headers.Add("ETag", new StringValues(
                new string[] {
                    hash,
                }));
        }


    }
}
