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
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Web.Shared.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Web.Shared.FlowElements
{
    /// <summary>
    /// Flow Element that is used to build complete JavaScript code
    /// files from specific properties within an <see cref="IFlowData"/>
    /// instance.
    /// The JavaScript is intended for execution on the client device
    /// where it can perform various actions (e.g. setting cookies)
    /// to provide more evidence to the Pipeline on the next request.
    /// </summary>
    public class JavaScriptBundlerElement
        : FlowElementBase<JavaScriptData, IElementPropertyMetaData>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger to use
        /// </param>
        /// <param name="elementDataFactory">
        /// The factory function to use when creating 
        /// <see cref="JavaScriptBundlerData"/> instances .
        /// </param>
        public JavaScriptBundlerElement(
            ILogger<FlowElementBase<JavaScriptData, IElementPropertyMetaData>> logger, 
            Func<IPipeline, FlowElementBase<JavaScriptData, IElementPropertyMetaData>, JavaScriptData> elementDataFactory)
            : base(logger, elementDataFactory)
        {
            _properties = new List<IElementPropertyMetaData>()
            {
                new ElementPropertyMetaData(this, Constants.CLIENTSIDE_JAVASCRIPT_PROPERTY_NAME, typeof(string), true)
            };
        }

        /// <summary>
        /// The key to access the element data within <see cref="IFlowData"/>.
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

        /// <summary>
        /// Use the element's logic to process the specified flow data.
        /// </summary>
        /// <param name="data">
        /// The flow data to process.
        /// </param>
        protected override void ProcessInternal(IFlowData data)
        {
            StringBuilder completeJavaScript = new StringBuilder();

            // Start the class declaration, including constructor
            completeJavaScript.AppendLine(@"class FOD_CO { ");
            completeJavaScript.AppendLine(@"constructor() {};");

            var javaScriptProperties = data
                .GetWhere(p => p.Type == typeof(JavaScript));

            // Add each JavaScript property value as a method on the class.
            foreach (var javaScriptProperty in 
                javaScriptProperties.Where(p => p.Value != null))
            {
                bool addToJS = true;
                if (typeof(IAspectPropertyValue).IsAssignableFrom(javaScriptProperty.Value.GetType()))
                {
                    addToJS = (javaScriptProperty.Value as IAspectPropertyValue).HasValue;
                }

                if (addToJS)
                {
                    completeJavaScript.Append(
                        javaScriptProperty.Key
                        .Replace('.', '_')
                        .Replace('-', '_'));
                    completeJavaScript.AppendLine("() {");
                    completeJavaScript.AppendLine(javaScriptProperty.Value.ToString());
                    completeJavaScript.AppendLine("}");
                }
            }
            completeJavaScript.AppendLine("}");

            // Add code to create an instance of the class and call each of 
            // the methods on it.
            completeJavaScript.AppendLine("let fod_co = new FOD_CO();");
            foreach (var javaScriptProperty in javaScriptProperties)
            {
                completeJavaScript.Append("fod_co.");
                completeJavaScript.Append(
                    javaScriptProperty.Key
                    .Replace('.', '_')
                    .Replace('-', '_'));
                completeJavaScript.AppendLine("();");
            }

            // Create the element data and add it to the flow data.
            var elementData = data.GetOrAdd(ElementDataKeyTyped, CreateElementData);
            elementData.JavaScript = completeJavaScript.ToString();
            
        }

        protected override void ManagedResourcesCleanup()
        {
        }
        protected override void UnmanagedResourcesCleanup()
        {
        }
    }
}
