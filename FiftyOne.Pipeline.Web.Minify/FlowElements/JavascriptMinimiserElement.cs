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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Web.Shared;
using FiftyOne.Pipeline.Web.Shared.Data;
using FiftyOne.Pipeline.Web.Shared.FlowElements;
using Microsoft.Extensions.Logging;
using NUglify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Web.Minify.FlowElements
{
    /// <summary>
    /// A flow element that takes the JavaScript property from
    /// <see cref="JavaScriptData"/> and runs it through a minimiser. 
    /// The resulting script is used to update the same property.
    /// </summary>
    public class JavaScriptMinimiserElement : 
        FlowElementBase<JavaScriptData, IElementPropertyMetaData>
    {
        /// <summary>
        /// True if the last request caused an error in the minimiser.
        /// This is used to prevent massive log spam (this component can 
        /// create very large log entries) if sequential calls cause the
        /// same error.
        /// </summary>
        private bool _lastRequestWasError;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger to use
        /// </param>
        public JavaScriptMinimiserElement(
            ILogger<JavaScriptMinimiserElement> logger)
            : base(logger)
        {
            _logger = logger;

            _properties = new List<IElementPropertyMetaData>()
            {
                new ElementPropertyMetaData(this, Constants.CLIENTSIDE_JAVASCRIPT_PROPERTY_NAME, typeof(string), true)
            };
        }

        /// <summary>
        /// The key used to access the element data within FlowData.
        /// </summary>
        public override string ElementDataKey => Constants.CLIENTSIDE_JAVASCRIPT_DATA_KEY;

        /// <summary>
        /// This element uses no evidence values so the filter is an
        /// empty white list.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <summary>
        /// The properties this element populates.
        /// </summary>
        private List<IElementPropertyMetaData> _properties;
        /// <summary>
        /// The properties this element populates
        /// </summary>
        public override IList<IElementPropertyMetaData> Properties => _properties;


        protected override void ProcessInternal(IFlowData data)
        {
            // Get the bundled JavaScript to be minimised.
            // We could use the TypedDataKey from this instance but instead
            // use the one from the JavaScriptBundler instance to ensure we
            // are using the right key (e.g. if bundler is changed to use
            // a different key for some reason)
            var bundlerElement = data.Pipeline.GetElement<JavaScriptBundlerElement>();
            if(bundlerElement == null)
            {
                throw new Exception("The JavaScriptMinimiserElement can only " +
                    "be used if the pipeline also contains a " +
                    "JavaScriptBundlerElement.");
            }
            var elementData = data.GetFromElement(bundlerElement);
            string minifiedJavaScript = "";

            // Minimise the script.
            var ugly = Uglify.Js(elementData.JavaScript);

            if (ugly.HasErrors)
            {
                // If there were are errors then log them and
                // return the non-minified response.
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
                    errorText.AppendLine(elementData.JavaScript);
                    _logger.LogError(errorText.ToString());
                    _lastRequestWasError = true;
                }
            }
            else
            {
                minifiedJavaScript = ugly.Code;
            }

            // Create the element data and add it to the flow data.
            elementData.JavaScript = minifiedJavaScript;
        }

        protected override void ManagedResourcesCleanup()
        {
        }

        protected override void UnmanagedResourcesCleanup()
        {
        }
    }
}
