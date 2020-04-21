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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiftyOne.Pipeline.Web.Framework.Providers
{
    /// <summary>
    /// The provider that serves the JavaScript when requested.
    /// </summary>
    internal class FiftyOneJsProvider
    {
        /// <summary>
        /// The single instance of the provider.
        /// </summary>
        private static FiftyOneJsProvider _instance = null;

        /// <summary>
        /// Lock used when constructing the instance.
        /// </summary>
        private static readonly object _lock = new object();
        
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
        /// Get the single instance of the provider. If one does not yet
        /// exist, it is constructed.
        /// </summary>
        /// <returns></returns>
        public static FiftyOneJsProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FiftyOneJsProvider();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Create a new FiftyOneJsProvider
        /// </summary>
        public FiftyOneJsProvider()
        {
            var pipeline = WebPipeline.GetInstance().Pipeline;
            var headersAffectingJavaScript = new List<string>();
            // Get evidence filters for all elements that have
            // JavaScript properties.
            var filters = pipeline.FlowElements
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
            context.Response.Clear();
            context.Response.ClearHeaders();

            PipelineCapabilities caps = context.Request.Browser as PipelineCapabilities;

            var flowData = caps.FlowData;

            // Get the hash code.
            var hash = flowData.GenerateKey(
                flowData.Pipeline.EvidenceKeyFilter).GetHashCode();

            if (hash.ToString() == context.Request.Headers["If-None-Match"])
            {
                // The response hasn't changed so respond with a 304.
                context.Response.StatusCode = 304;
            }
            else
            {
                var bundler = flowData.Pipeline.GetElement<JavaScriptBuilderElement>();
                if (bundler != null)
                {
                    var bundlerData = flowData.GetFromElement(bundler);

                    // Otherwise, return the minified script to the client.
                    context.Response.Write(bundlerData.JavaScript);

                    SetHeaders(context, hash.ToString(), bundlerData.JavaScript.Length);
                }
                else
                {
                    // There is no bundler element to get the javascript from
                    // so the response will just be empty.
                }
            }
            context.ApplicationInstance.CompleteRequest();
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
            context.Response.AddHeader("Content-Length", contentLength.ToString());
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("Cache-Control", _cacheControl);
            if (string.IsNullOrEmpty(_headersAffectingJavaScript.ToString()) == false)
            {
                context.Response.Headers.Add("Vary", _headersAffectingJavaScript);
            }
            context.Response.Headers.Add("ETag", new StringValues(
                new string[] {
                    hash,
                }));
        }
    }
}
