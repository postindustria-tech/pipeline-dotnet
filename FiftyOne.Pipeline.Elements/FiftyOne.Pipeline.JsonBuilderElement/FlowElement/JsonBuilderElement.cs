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
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Engines.FiftyOne;

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
        private HashSet<string> _elementBlacklist;

        // An array of custom converters to use when serializing 
        // the property values.
        private static JsonConverter[] JSON_CONVERTERS = new JsonConverter[]
        {
            new JavaScriptConverter(),
            new AspectPropertyValueConverter()
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger for this instance to use.
        /// </param>
        /// <param name="jsonConverters">
        /// A collection of <see cref="JsonConverter"/> instances that will
        /// be used when serializing the data.
        /// </param>
        /// <param name="elementDataFactory">
        /// The factory function to use when creating a new element data
        /// instance.
        /// </param>
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
            // Blacklist of the element data keys of elements that should 
            // not be added to the Json.
            _elementBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { "cloud-response", "json-builder" };

            JSON_CONVERTERS = JSON_CONVERTERS.Concat(jsonConverters).ToArray();
        }

        /// <summary>
        /// The key to identify this engine's element data instance
        /// within <see cref="IFlowData"/>.
        /// </summary>
        public override string ElementDataKey => "json-builder";

        /// <summary>
        /// A filter that identifies the evidence items that this 
        /// element can make use of.
        /// JsonBuilder does not make use of any evidence so this filter
        /// will never match anything.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter => 
            _evidenceKeyFilter;

        /// <summary>
        /// The meta-data for the properties populated by this element.
        /// </summary>
        public override IList<IElementPropertyMetaData> Properties => 
            _properties;

        /// <summary>
        /// Cleanup of an managed resources.
        /// </summary>
        protected override void ManagedResourcesCleanup()
        { }

        /// <summary>
        /// Transform the data in the flow data instance into a
        /// JSON object.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the supplied flow data is null.
        /// </exception>
        protected override void ProcessInternal(IFlowData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

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
        /// <returns>
        /// A string containing the data in JSON format.
        /// </returns>
        protected virtual string BuildJson(IFlowData data)
        {
            int sequenceNumber = GetSequenceNumber(data);

            // Get property values from all the elements and add the ones that
            // are accessible to a dictionary.
            Dictionary<String, object> allProperties = GetAllProperties(data);

            // Only populate the JavaScript properties if the sequence 
            // has not reached max iterations.
            if (sequenceNumber < Constants.MAX_JAVASCRIPT_ITERATIONS)
            {
                AddJavaScriptProperties(data, allProperties);
            }

            AddErrors(data, allProperties);

            return BuildJson(allProperties);
        }

        /// <summary>
        /// Build the JSON 
        /// </summary>
        /// <param name="allProperties">
        /// A dictionary containing the data to convert to JSON.
        /// Key is the element data key.
        /// Value is a <code><![CDATA[Dictionary<string, object>]]></code>
        /// containing the property names and values for that element.
        /// </param>
        /// <returns></returns>
        protected static string BuildJson(Dictionary<string, object> allProperties)
        {
            // Build the JSON object from the property list containing property 
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
        /// <exception cref="ArgumentNullException">
        /// Thrown if one of the supplied parameters is null
        /// </exception>
        protected static void AddErrors(IFlowData data,
            Dictionary<String, object> allProperties)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (allProperties == null) throw new ArgumentNullException(nameof(allProperties));

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
        /// <exception cref="ArgumentNullException">
        /// Thrown if the supplied flow data is null.
        /// </exception>
        /// <exception cref="PipelineException">
        /// Thrown if sequence number is not present in the evidence.
        /// </exception>
        protected static int GetSequenceNumber(IFlowData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (data.TryGetEvidence(Engines.FiftyOne.Constants.EVIDENCE_SEQUENCE, out int sequence) == false)
            {
                throw new PipelineException(Messages.ExceptionSequenceNumberNotPresent);
            }
            return sequence;
        }

        /// <summary>
        /// Cleanup any unmanaged resources
        /// </summary>
        protected override void UnmanagedResourcesCleanup()
        { }


        /// <summary>
        /// Get all the proeprties.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the supplied flow data is null.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", 
            "CA1308:Normalize strings to uppercase", 
            Justification = "Pipeline API specification is for JSON " +
            "data to always use fully lower-case keys")]
        protected virtual Dictionary<String, object> GetAllProperties(IFlowData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, object> allProperties = new Dictionary<string, object>();

            foreach (var element in data.ElementDataAsDictionary().Where(elementData => 
                _elementBlacklist.Contains(elementData.Key) == false))
            {
                if (allProperties.ContainsKey(element.Key.ToLowerInvariant()) == false)
                {
                    var values = GetValues((element.Value as IElementData).AsDictionary());
                    allProperties.Add(element.Key.ToLowerInvariant(), values);
                }
            }

            return allProperties;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Pipeline API specification is for JSON " +
            "data to always use fully lower-case keys")]
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
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance to get properties from.
        /// </param>
        /// <param name="allProperties">
        /// A collection of the properties that are accessible in the
        /// current request.
        /// Key is the element data key. 
        /// Value is a <code><![CDATA[Dictionary<string, object>]]></code>
        /// that contains the property names and values.
        /// </param>
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

        /// <summary>
        /// Get the names of all properties from the given 
        /// <see cref="IFlowData"/> instance that have type 'JavaScript'.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> to get properties from.
        /// </param>
        /// <param name="availableProperties">
        /// A list of the full string names (i.e. prefixed with the element 
        /// data key for the element that populates that property) of the 
        /// available properties in dot-separated format.
        /// </param>
        /// <returns>
        /// A list of the full string names of the properties that are of 
        /// type 'JavaScript'
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the supplied flow data is null.
        /// </exception>
        protected virtual IList<string> GetJavaScriptProperties(
            IFlowData data,
            IList<string> availableProperties)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

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
