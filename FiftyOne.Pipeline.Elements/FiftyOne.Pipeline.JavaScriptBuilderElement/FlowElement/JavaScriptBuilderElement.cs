/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using FiftyOne.Pipeline.JavaScriptBuilder.Templates;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using NUglify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using FiftyOne.Pipeline.JsonBuilder.FlowElement;
using FiftyOne.Pipeline.JsonBuilder.Data;

namespace FiftyOne.Pipeline.JavaScriptBuilder.FlowElement
{
    /// <summary>
    /// JavaScript Builder Element generates a JavaScript include to be run on 
    /// the client device.
    /// </summary>
	public class JavaScriptBuilderElement : 
        FlowElementBase<IJavaScriptBuilderElementData, IElementPropertyMetaData>
	{
        /// <summary>
        /// Evidence filter containing required evidence.
        /// </summary>
		private IEvidenceKeyFilter _evidenceKeyFilter;

        /// <summary>
        /// Properties returned by the engine.
        /// </summary>
		private IList<IElementPropertyMetaData> _properties;

        protected string _host = string.Empty;
        protected bool _overrideHost = false;
        protected string _endpoint = string.Empty;
        protected string _protocol = Constants.DEFAULT_PROTOCOL;
        protected bool _overrideProtocol;
        protected string _objName;

        /// <summary>
        /// Flag set if last request resulted in an error, stops processing if 
        /// true.
        /// </summary>
        private bool _lastRequestWasError;

        /// <summary>
        /// Key to identify engine.
        /// </summary>
        public override string ElementDataKey
		{
			get { return "javascriptbuilderelement"; }
		}

        /// <summary>
        /// Publicly accessible EvidenceKeyFilter
        /// </summary>
		public override IEvidenceKeyFilter EvidenceKeyFilter => 
			_evidenceKeyFilter;

        /// <summary>
        /// Publicly accessible Property list.
        /// </summary>
		public override IList<IElementPropertyMetaData> Properties => 
			_properties;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="elementDataFactory"></param>
        /// <param name="host"></param>
        /// <param name="overrideHost"></param>
        /// <param name="endpoint"></param>
        /// <param name="protocol"></param>
        /// <param name="overrideProtocol"></param>
        /// <param name="objectName"></param>
		public JavaScriptBuilderElement(
			ILogger<JavaScriptBuilderElement> logger,
			Func<IFlowData, 
                FlowElementBase<IJavaScriptBuilderElementData, IElementPropertyMetaData>,
			    IJavaScriptBuilderElementData> elementDataFactory,
            string host,
            bool overrideHost,
            string endpoint,
            string protocol,
            bool overrideProtocol,
            string objectName)
			: base(logger, elementDataFactory)
		{
            // Set the evidence key filter for the flow data to use.
            _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(
                new List<string>() {
                    Constants.EVIDENCE_HOST_KEY,
                    Constants.EVIDENCE_PROTOCOL,
                    Constants.EVIDENCE_OBJECT_NAME
                });

            _properties = new List<IElementPropertyMetaData>()
				{
					new ElementPropertyMetaData(
						this,
                        "javascript",
                        typeof(string),
                        true)
				};

            _host = host;
            _overrideHost = overrideHost;
            _endpoint = endpoint;
            _protocol = protocol;
            _overrideProtocol = overrideProtocol;
            _objName = string.IsNullOrEmpty(objectName) ? "fod" : objectName;
		}

		protected override void ManagedResourcesCleanup()
		{ }

        /// <summary>
        /// Default process method.
        /// </summary>
        /// <param name="data"></param>
		protected override void ProcessInternal(IFlowData data)
		{
            SetUp(data);
        }

        private void SetUp(IFlowData data)
        {
            var host = string.Empty;
            var protocol = string.Empty;
            bool supportsPromises;

            // Try and get the request host name so it can be used to request
            // the Json refresh in the JavaScript code.
            if (_overrideHost == true)
            {
                if(data.TryGetEvidence(Constants.EVIDENCE_HOST_KEY, out host) == false)
                {
                    host = _host;
                }
            } else
            {
                host = _host;
            }

            // Try and get the request protocol so it can be used to request
            // the JSON refresh in the JavaScript code.
            if (_overrideProtocol)
            {
                if(data.TryGetEvidence(Constants.EVIDENCE_PROTOCOL, out protocol) == false)
                {
                    protocol = _protocol;
                }
            } else
            {
                protocol = _protocol;
            }

            // Could be for the requesting browser or the end-users browser.
            try
            {
                supportsPromises = data.GetAsString("Promise") == "Full";
            }
            catch (PipelineDataException)
            {
                supportsPromises = false;
            }

            // Get the JSON include to embed into the JavaScript include.
            string jsonObject = string.Empty;
            try
            {
                jsonObject = data.Get<IJsonBuilderElementData>().Json;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "The output of the Json builder is missing");
            }

            // Generate any required parameters for the JSON request.
            List<string> parameters = new List<string>();

            var queryEvidence = data
                .GetEvidence()
                .AsDictionary()
                .Where(e => e.Key.StartsWith(Core.Constants.EVIDENCE_QUERY_PREFIX))
                .Select(k => $"{k.Key.Remove(0, k.Key.IndexOf(Core.Constants.EVIDENCE_SEPERATOR) + 1)}={k.Value}");

            parameters.AddRange(queryEvidence);

            string queryParams = string.Join("&", parameters);
            string endpoint = _endpoint;

            string url = WebUtility.UrlEncode($"{protocol}://{host}/{endpoint}" +
                (String.IsNullOrEmpty(queryParams) ? "" : "?" + queryParams));

            // With the gathered resources, build a new JavaScriptResource.
            BuildJavaScript(data, jsonObject, supportsPromises, url);
		}

        protected void BuildJavaScript(IFlowData data, string jsonObject, bool supportsPromises, string url)
        {
            JavaScriptBuilderElementData elementData = (JavaScriptBuilderElementData)
                data.GetOrAdd(
                ElementDataKeyTyped,
                CreateElementData);

            // Try and get the requested object name from evidence.
            if (data.TryGetEvidence(Constants.EVIDENCE_OBJECT_NAME, out string objectName) == false ||
                string.IsNullOrWhiteSpace(objectName))
            {
                objectName = _objName;
            }

            JavaScriptResource javaScript = new JavaScriptResource(objectName, jsonObject,
                supportsPromises,
                url);

            string content = javaScript.TransformText();
            string minifiedContent = string.Empty;

            // Minimize the script.
            var ugly = Uglify.Js(content);

            if (ugly.HasErrors)
            {
                // If there were are errors then log them and
                // return the non-minified response.

                minifiedContent = content;

                if (_lastRequestWasError == false)
                {
                    StringBuilder errorText = new StringBuilder();
                    errorText.AppendLine("Errors occurred when minifying JavaScript.");
                    foreach (var error in ugly.Errors)
                    {
                        errorText.AppendLine($"{error.ErrorCode}: {error.Message}. " +
                            $"Line(s) {error.StartLine}-{error.EndLine}. " +
                            $"Column(s) {error.StartColumn}-{error.EndColumn}");
                    }
                    errorText.AppendLine(content);
                    _logger.LogError(errorText.ToString());
                    _lastRequestWasError = true;
                    data.Stop = true;
                }
            }
            else
            {
                minifiedContent = ugly.Code;
            }

#if DEBUG            
            // Undo minify in debug mode.
            minifiedContent = content;
#endif
            elementData.JavaScript = minifiedContent;
        }

		protected override void UnmanagedResourcesCleanup()
		{ }
	}
}
