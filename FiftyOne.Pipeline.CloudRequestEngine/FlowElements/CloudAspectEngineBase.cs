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
using System.Collections.Generic;
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
            private CloudRequestEngine _cloudRequestEngine = null;
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
            public CloudRequestEngine Instance
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

                                _cloudRequestEngine = _pipelinesAccessor()[0].GetElement<CloudRequestEngine>();

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
        private bool LoadAspectProperties(CloudRequestEngine engine)
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
                    var property = new AspectPropertyMetaData(this,
                        item.Name,
                        item.GetPropertyType(),
                        item.Category,
                        new List<string>(),
                        true);
                    _aspectProperties.Add(property);
                }
                return true;
            }
            else
            {
                _logger.LogError($"Aspect properties could not be " +
                    $"loaded for {GetType().Name}", this);
                return false;
            }
        }
    }
}
