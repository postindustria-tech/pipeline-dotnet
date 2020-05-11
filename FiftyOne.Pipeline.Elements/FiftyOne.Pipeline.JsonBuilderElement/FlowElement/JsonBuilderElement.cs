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

using FiftyOne.Pipeline.JsonBuilder.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.JsonBuilder.Converters;

namespace FiftyOne.Pipeline.JsonBuilder.FlowElement
{
    /// <summary>
    /// The JsonBuilderElement takes accessible properties and adds the property
    /// key:values to the Json object. The element will also add any errors 
    /// which have been recorded in the FlowData.
    /// </summary>
    public class JsonBuilderElement : 
        FlowElementBase<IJsonBuilderElementData, IElementPropertyMetaData>, 
        IJsonBuilderElement
    {
        private EvidenceKeyFilterWhitelist _evidenceKeyFilter;
        private List<IElementPropertyMetaData> _properties;
        private List<string> _blacklist;
               
        // An array of custom converters to use when seralising 
        // the property values.
        private static JsonConverter[] JSON_CONVERTERS = new JsonConverter[]
        {
            new JavaScriptConverter(),
            new AspectPropertyValueConverter()
        };

        public JsonBuilderElement(
            ILogger<JsonBuilderElement> logger,
            IEnumerable<JsonConverter> jsonConverters,
            Func<IPipeline, FlowElementBase<IJsonBuilderElementData, IElementPropertyMetaData>,
                IJsonBuilderElementData> elementDataFactory)
            : base(logger, elementDataFactory)
        {
            // Set the evidence key filter for the flow data to use.
            _evidenceKeyFilter = 
                new EvidenceKeyFilterWhitelist(new List<string>() { });

            _properties = new List<IElementPropertyMetaData>()
                {
                    new ElementPropertyMetaData(
                        this, "json", typeof(string), true)
                };

            // Blacklist of properties which should not be added to the Json.
            _blacklist = new List<string>() { "products", "properties" };

            JSON_CONVERTERS = JSON_CONVERTERS.Concat(jsonConverters).ToArray();
        }

        public override string ElementDataKey => "json-builder";

        public override IEvidenceKeyFilter EvidenceKeyFilter => 
            _evidenceKeyFilter;

        public override IList<IElementPropertyMetaData> Properties => 
            _properties;

        protected override void ManagedResourcesCleanup()
        { }

        protected override void ProcessInternal(IFlowData data)
        {
            var elementData = data.GetOrAdd(
                    ElementDataKeyTyped,
                    CreateElementData);
            var jsonString = BuildJson(data);
            elementData.Json = jsonString;
        }

        /// <summary>
        /// Create and populate a JSON string from the specified data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="allProperties"></param>
        /// <param name="javascriptProperties"></param>
        /// <returns>
        /// A string containing the data in JSON format.
        /// </returns>
        protected virtual string BuildJson(IFlowData data)
        {
            int sequenceNumber = GetSequenceNumber(data);

            // Get property values from all the elements and add the ones that
            // are accessible to a dictionary.
            Dictionary<String, object> allProperties = GetAllProperties(data);

            // Only populate the javascript properties if the sequence 
            // has not reached max iterations.
            if (sequenceNumber < Constants.MAX_JAVASCRIPT_ITERATIONS)
            {
                AddJavaScriptProperties(data, allProperties);
            }

            AddErrors(data, allProperties);

            return BuildJson(allProperties);
        }

        protected string BuildJson(Dictionary<string, object> allProperties)
        {
            // Build the Json object from the property list containing property 
            // values and errors.
            // TODO: Remove formatting
            return JsonConvert.SerializeObject(allProperties,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = JSON_CONVERTERS
                });
        }

        /// <summary>
        /// Add any JavaScript properties to the dictionary
        /// </summary>
        /// <param name="data"></param>
        /// <param name="allProperties"></param>
        private void AddJavaScriptProperties(IFlowData data,
            Dictionary<String, object> allProperties)
        {
            var javascriptProperties = GetJavaScriptProperties(data, allProperties);
            if (javascriptProperties != null &&
                javascriptProperties.Count > 0)
            {
                allProperties.Add("javascriptProperties", javascriptProperties);
            }
        }

        /// <summary>
        /// Add any errors in the flow data object to the dictionary
        /// </summary>
        /// <param name="data"></param>
        /// <param name="allProperties"></param>
        protected void AddErrors(IFlowData data,
            Dictionary<String, object> allProperties)
        {
            // If there are any errors then add them to the Json.
            if (data.Errors != null && data.Errors.Count > 0)
            {
                var errors = new Dictionary<string, List<string>>();
                foreach (var error in data.Errors)
                {
                    if (errors.ContainsKey(error.FlowElement.ElementDataKey))
                    {
                        errors[error.FlowElement.ElementDataKey].Add(error.ExceptionData.Message);
                    }
                    else
                    {
                        errors.Add(error.FlowElement.ElementDataKey,
                            new List<string>() { error.ExceptionData.Message });
                    }
                }
                allProperties.Add("errors", errors);
            }
        }

        /// <summary>
        /// Get the sequence number from the evidence.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected int GetSequenceNumber(IFlowData data)
        {
            int sequence = 1;
            if (data.TryGetEvidence("query.sequence", out sequence) == false)
            {
                throw new Exception("Sequence number not present in evidence. " +
                    "this is mandatory.");
            }
            return sequence;
        }

        protected override void UnmanagedResourcesCleanup()
        { }

        /// <summary>
        /// Get all the proeprties.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Dictionary<String, object> GetAllProperties(IFlowData data)
        {
            Dictionary<string, object> allProperties = new Dictionary<string, object>();

            foreach (var element in data.ElementDataAsDictionary())
            {
                if (allProperties.ContainsKey(element.Key.ToLowerInvariant()) == false)
                {
                    var values = GetValues((element.Value as IElementData).AsDictionary());

                    allProperties.Add(element.Key.ToLowerInvariant(), values);
                }
            }

            return allProperties;
        }

        private Dictionary<string, object> GetValues(IReadOnlyDictionary<string, object> readOnlyDictionary)
        {
            var values = new Dictionary<string, object>();

            foreach(var value in readOnlyDictionary)
            {
                if(value.Value is IAspectPropertyValue aspectProperty)
                {
                    if (aspectProperty.HasValue) 
                    {
                        if(aspectProperty.Value is IList elementDatas &&
                            aspectProperty.Value.GetType().GetElementType() == typeof(IElementData))
                        {
                            var results = new List<object>();
                            foreach(var elementData in elementDatas)
                            {
                                results.Add(GetValues(((IElementData)elementData).AsDictionary()));
                            }
                            values.Add(value.Key.ToLowerInvariant(), results);
                        }
                        else
                        {
                            values.Add(value.Key.ToLowerInvariant(), aspectProperty.Value);
                        } 
                    }
                    else
                    {
                        values.Add(value.Key.ToLowerInvariant(), null);
                        values.Add(value.Key.ToLowerInvariant() + "nullreason", 
                            aspectProperty.NoValueMessage);
                    }
                } 
                else
                {
                    values.Add(value.Key.ToLowerInvariant(), value.Value);
                }
            }
            return values;
        }

        /// <summary>
        /// Get accessible JavaScript properties and the property keys 
        /// (property names) to a list which will be used by the JavaScript
        /// resource to determine if they should be run.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="propertyWhitelist"></param>
        /// <returns></returns>
        private IList<string> GetJavaScriptProperties(
            IFlowData data,
            Dictionary<string, object> allProperties)
        {
            // Create a list of the available properties in the form of 
            // "elementdatakey.property" from a 
            // Dictionary<string, Dictionary<string, object>> of properties
            // structured as <element<prefix,prop>>  
            var props = new List<string>();

            foreach(var element in allProperties)
                foreach (var property in (Dictionary<string, object>)element.Value)
                    props.Add(element.Key + Core.Constants.EVIDENCE_SEPERATOR + property.Key);

            return GetJavaScriptProperties(data, props);
        }

        protected virtual IList<string> GetJavaScriptProperties(
            IFlowData data,
            IList<string> availableProperties)
        {
            // Get a list of all the JavaScript properties which are available.
            var javascriptPropertiesEnumerable =
                data.GetWhere(
                    p => p.Type != null &&
                    (p.Type.Equals(typeof(JavaScript)) ||
                    p.Type.Equals(typeof(IAspectPropertyValue<JavaScript>))))
                    .Where(p => availableProperties.Contains(p.Key));

            List<string> javascriptPropeties = new List<string>();
            foreach (var property in javascriptPropertiesEnumerable)
            {
                javascriptPropeties.Add(property.Key);
            }
            return javascriptPropeties;
        } 
    }
}
