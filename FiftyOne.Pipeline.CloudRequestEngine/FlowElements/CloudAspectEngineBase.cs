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
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.FlowElements
{
    /// <summary>
    /// Base class for 51Degrees cloud aspect engines. 
    /// </summary>
    /// <typeparam name="T">
    /// The type of data that the engine will return. Must implement 
    /// <see cref="IAspectData"/>.
    /// </typeparam>
    public abstract class CloudAspectEngineBase<T> : 
        AspectEngineBase<T, IAspectPropertyMetaData>, 
        ICloudAspectEngine
        where T : IAspectData
    {
        /// <summary>
        /// Internal class that is used to retrieve the
        /// <see cref="CloudRequestEngine"/> instance that will be making
        /// requests on behalf of this engine.
        /// </summary>
        protected class RequestEngineAccessor
        {
            private ICloudRequestEngine _cloudRequestEngine = null;
            private object _cloudRequestEngineLock = new object();
            private Func<IReadOnlyList<IPipeline>> _pipelinesAccessor;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="pipelinesAccessor">
            /// A function that returns the list of pipelines associated 
            /// with the parent engine.
            /// </param>
            public RequestEngineAccessor(Func<IReadOnlyList<IPipeline>> pipelinesAccessor)
            {
                _pipelinesAccessor = pipelinesAccessor;
            }

            /// <summary>
            /// Get the <see cref="CloudRequestEngine"/> that will be making
            /// requests on behalf of this engine.
            /// </summary>
            public ICloudRequestEngine Instance
            {
                get
                {
                    if (_cloudRequestEngine == null)
                    {
                        lock (_cloudRequestEngineLock)
                        {
                            if (_cloudRequestEngine == null)
                            {
                                if (_pipelinesAccessor().Count > 1)
                                {
                                    throw new PipelineConfigurationException(
                                        $"'{GetType().Name}' does not support being " +
                                        $"added to multiple Pipelines.");
                                }
                                if (_pipelinesAccessor().Count == 0)
                                {
                                    throw new PipelineConfigurationException(
                                        $"'{GetType().Name}' has not yet been added " +
                                        $"to a Pipeline.");
                                }

                                _cloudRequestEngine = _pipelinesAccessor()[0].GetElement<ICloudRequestEngine>();

                                if (_cloudRequestEngine == null)
                                {
                                    throw new PipelineConfigurationException(
                                        $"The '{GetType().Name}' requires a 'CloudRequestEngine' " +
                                        $"before it in the Pipeline. This engine will be unable " +
                                        $"to produce results until this is corrected.");
                                }
                            }
                        }
                    }
                    return _cloudRequestEngine;
                }
            }
        }

        private IList<IAspectPropertyMetaData> _aspectProperties;
        private object _aspectPropertiesLock = new object();
        private string _dataSourceTier;

        public override string DataSourceTier => _dataSourceTier;

        /// <summary>
        /// Used to access the <see cref="CloudRequestEngine"/> that will
        /// be making requests on behalf of this engine.
        /// </summary>
        protected RequestEngineAccessor RequestEngine { get; set; }

        /// <summary>
        /// Get property meta-data for properties populated by this engine
        /// </summary>
        public override IList<IAspectPropertyMetaData> Properties
        {
            get
            {
                if (_aspectProperties == null)
                {
                    lock (_aspectPropertiesLock)
                    {
                        if (_aspectProperties == null)
                        {
                            if (LoadAspectProperties(RequestEngine.Instance) == false)
                            {
                                throw new Exception("Failed to load aspect properties");
                            }
                        }
                    }
                }
                return _aspectProperties;
            }
        }

        public CloudAspectEngineBase(ILogger<AspectEngineBase<T, IAspectPropertyMetaData>> logger, 
            Func<IPipeline, FlowElementBase<T, IAspectPropertyMetaData>, T> aspectDataFactory) : base(logger, aspectDataFactory)
        {
            RequestEngine = new RequestEngineAccessor(() => Pipelines);
        }
        
        protected override void UnmanagedResourcesCleanup()
        {
        }

        /// <summary>
        /// Get property meta data from the <see cref="CloudRequestEngine"/> 
        /// for properties relating to this engine instance.
        /// This method will populate the <see cref="_aspectProperties"/>
        /// field.
        /// </summary>
        /// <remarks>
        /// There will be one <see cref="CloudRequestEngine"/> in a
        /// Pipeline that makes the actual web requests to the cloud service.
        /// One or more cloud aspect engines will take the response from these
        /// cloud requests and convert them into strongly typed objects.
        /// Given this model, the cloud aspect engines have no knowledge
        /// of which properties the <see cref="CloudRequestEngine"/> can
        /// return.
        /// They must extract the properties relevant to them from the
        /// meta-data for all properties that the 
        /// <see cref="CloudRequestEngine"/> exposes.
        /// </remarks>
        /// <param name="engine">
        /// The <see cref="CloudRequestEngine"/> from which to retrieve
        /// property meta-data.
        /// </param>
        /// <returns>
        /// True if the _aspectProperties has been successfully populated
        /// with the relevant property meta-data.
        /// False if something has gone wrong.
        /// </returns>
        private bool LoadAspectProperties(ICloudRequestEngine engine)
        {
            var dictionary = engine.PublicProperties;

            if (dictionary != null &&
                dictionary.Count > 0 &&
                dictionary.ContainsKey(ElementDataKey))
            {
                _aspectProperties = new List<IAspectPropertyMetaData>();
                _dataSourceTier = dictionary[ElementDataKey].DataTier;

                foreach (var item in dictionary[ElementDataKey].Properties)
                {
                    var property = LoadProperty(item);
                    if (property != null)
                    {
                        _aspectProperties.Add(property);
                    }
                }

                return _aspectProperties.Count > 0;
            }
            else
            {
                _logger.LogError($"Aspect properties could not be " +
                    $"loaded for {GetType().Name}", this);
                return false;
            }
        }

        /// <summary>
        /// Translate the specified property from <see cref="PropertyMetaData"/>
        /// to <see cref="AspectPropertyMetaData"/>.
        /// </summary>
        /// <param name="property">
        /// The <see cref="PropertyMetaData"/> instance to translate.
        /// </param>
        /// <param name="parentObjectType">
        /// The type of the object on which this property exists.
        /// </param>
        /// <returns>
        /// A new <see cref="AspectPropertyMetaData"/> instance, created from
        /// the values in the supplied <see cref="PropertyMetaData"/>.
        /// </returns>
        private AspectPropertyMetaData LoadProperty(PropertyMetaData property, Type parentObjectType = null)
        {
            // If parent object type is not set then use the type of the
            // data returned by this engine.
            if(parentObjectType == null)
            {
                parentObjectType = typeof(T);
            }
            // Get the property info for this property based on the 
            // supplied name.
            var propertyInfo = parentObjectType.GetProperty(property.Name, 
                BindingFlags.Public | 
                BindingFlags.Instance | 
                BindingFlags.IgnoreCase);

            if (propertyInfo != null) 
            {
                // Load any sub properties.
                List<AspectPropertyMetaData> subProperties = null;
                if (property.ItemProperties != null &&
                    property.ItemProperties.Count > 0)
                {
                    subProperties = new List<AspectPropertyMetaData>();
                    var thisType = propertyInfo.PropertyType;
                    if (typeof(IEnumerable).IsAssignableFrom(thisType) &&
                        thisType.IsGenericType)
                    {
                        // Get the type of the items in this list so
                        // LoadProperty can use reflection to get its
                        // properties.
                        var itemType = thisType.GetGenericArguments()[0];
                        foreach (var subproperty in property.ItemProperties)
                        {
                            var newProperty = LoadProperty(subproperty, itemType);
                            if (newProperty != null)
                            {
                                subProperties.Add(newProperty);
                            }
                        }
                    }
                    else 
                    {
                        _logger.LogWarning($"Problem parsing sub-items. " +
                            $"Property '{parentObjectType.Name}.{property.Name}' " +
                            $"does not implement IEnumerable<>.");
                    }
                }

                // Create the AspectPropertyMetaData instance.
                return new AspectPropertyMetaData(this,
                    property.Name,
                    propertyInfo.PropertyType,
                    property.Category,
                    new List<string>(),
                    true,
                    "",
                    subProperties);
            }
            else
            {
                // Could not find a matching property on the parent object
                // so log a warning.
                _logger.LogWarning($"Failed to find property '{property.Name}' " +
                    $"on data object '{parentObjectType.Name}'. ");
                return null;
            }
        }

        /// <summary>
        /// Use the supplied cloud data to create a dictionary of 
        /// <see cref="AspectPropertyValue{T}"/> instances.
        /// </summary>
        /// <remarks>
        /// This method uses the meta-data exposed by the 
        /// <see cref="Properties"/> collection to determine
        /// if a given entry in the supplied cloudData should
        /// be converted to an <see cref="AspectPropertyValue{T}"/>
        /// or not.
        /// If not it will be output unchanged. If it is then a new
        /// <see cref="AspectPropertyValue{T}"/> instance will be created 
        /// and the value from the cloud data assigned to it.
        /// If the value is null then the code will look for a property
        /// in the cloud data with the same name suffixed with 'nullreason'.
        /// If it exists, then it's value will be used to set the 
        /// noValueMessage on the new <see cref="AspectPropertyValue{T}"/>.
        /// </remarks>
        /// <param name="cloudData">
        /// The cloud data to be processed.
        /// Keys are flat property names (i.e. no '.' separators).
        /// Values are the property values.
        /// </param>
        /// <param name="propertyMetaData">
        /// The meta-data for the properties in the data.
        /// This will usually be the list from <see cref="Properties"/>
        /// but will be different if dealing with sub-properties.
        /// </param>
        /// <returns>
        /// A dictionary containing the original values converted to 
        /// <see cref="AspectPropertyValue{T}"/> instances where needed.
        /// Any entries in the source dictionary where the key ends 
        /// with 'nullreason' will not appear in the output.
        /// </returns>
        protected Dictionary<string, object> CreateAPVDictionary(
            Dictionary<string, object> cloudData,
            IReadOnlyList<IElementPropertyMetaData> propertyMetaData)
        {
            // Convert the meta-data to a dictionary for faster access.
            var metaDataDictionary = propertyMetaData.ToDictionary(
                p => p.Name, p => p, 
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, object> result = new Dictionary<string, object>();
            // Iterate through all entries in the source data where the
            // key is not suffixed with 'nullreason'.
            foreach (var property in cloudData
                .Where(kvp => kvp.Key.EndsWith("nullreason") == false))
            {
                var outputValue = property.Value;

                if (metaDataDictionary.TryGetValue(property.Key, out IElementPropertyMetaData metaData) == true)
                {
                    if (typeof(IAspectPropertyValue).IsAssignableFrom(metaData.Type))
                    {
                        // If this property has a type of AspectPropertyValue
                        // then create a new instance and populate it.
                        var apvType = typeof(AspectPropertyValue<>);
                        var genericType = apvType.MakeGenericType(metaData.Type.GetGenericArguments());
                        object obj = Activator.CreateInstance(genericType);
                        var apv = obj as IAspectPropertyValue;
                        if (property.Value != null)
                        {
                            apv.Value = property.Value;
                        }
                        else 
                        { 
                            // Value is null so check if we have a 
                            // corresponding reason.
                            // We need to set the no value message with 
                            // reflection as the property is read only 
                            // through the interface.
                            var messageProperty = genericType.GetProperty("NoValueMessage");
                            if (cloudData.TryGetValue(property.Key + "nullreason", out object nullreason))
                            {
                                messageProperty.SetValue(apv, nullreason);
                            }
                            else
                            {
                                messageProperty.SetValue(apv, "Unknown");
                            }
                        }
                        outputValue = apv;
                    }
                }
                else
                {
                    _logger.LogWarning($"No meta-data entry for property " +
                        $"'{property.Key}' in '{GetType().Name}'");
                }

                result.Add(property.Key, outputValue);
            }
            return result;
        }
    }
}
