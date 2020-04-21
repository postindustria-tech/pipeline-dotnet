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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FiftyOne.Pipeline.Core.FlowElements
{
    /// <summary>
    /// A pipeline is used to create <see cref="IFlowData"/> instances
    /// which then automatically use the pipeline when their Process 
    /// method is called.
    /// </summary>
    public class Pipeline : IPipelineInternal
    {
        /// <summary>
        /// The pipeline maintains a dictionary of the elements it contains 
        /// indexed by type. This is used by the GetElement method.
        /// </summary>
        private Dictionary<Type, List<IFlowElement>> _elementsByType =
            new Dictionary<Type, List<IFlowElement>>();

        /// <summary>
        /// The pipeline maintains a dictionary of property meta data
        /// indexed by property name. This is used by the
        /// GetElementDataKeyForProperty method.
        /// </summary>
        private Dictionary<string, IElementPropertyMetaData> _metaDataByPropertyName = 
            new Dictionary<string, IElementPropertyMetaData>();

        /// <summary>
        /// A factory method that is used to create new 
        /// <see cref="IFlowData"/> instances.
        /// </summary>
        private Func<IPipelineInternal, IFlowData> _flowDataFactory;

        /// <summary>
        /// The <see cref="IFlowElement"/>s that make up this pipeline.
        /// </summary>
        private List<IFlowElement> _flowElements;

        /// <summary>
        /// A filter that will only include the evidence keys that can 
        /// be used by at least one <see cref="IFlowElement"/> within 
        /// this pipeline.
        /// (Will only be populated after the <see cref="EvidenceKeyFilter"/>
        /// property is used.)
        /// </summary>
        private EvidenceKeyFilterAggregator _evidenceKeyFilter;
        /// <summary>
        /// Provides an object to lock on when populating the 
        /// evidence key filter.
        /// </summary>
        private object _evidenceKeyFilterLock = new object();

        /// <summary>
        /// True if the instance is disposed. False otherwise.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// True if multiple <see cref="IFlowElement"/> instances will run 
        /// concurrently within this pipeline. False otherwise.
        /// (Will be null until the <see cref="IsConcurrent"/> property
        /// is used.)
        /// </summary>
        private bool? _concurrent = null;

        /// <summary>
        /// Control field that indicates if the Pipeline will automatically
        /// call Dispose on child elements when it is disposed or not.
        /// </summary>
        private bool _autoDisposeElements;

        /// <summary>
        /// Control field that indicates if the Pipeline will throw an
        /// aggregate exception during processing or suppress it and ignore the
        /// exceptions added to <see cref="IFlowData.Errors"/>.
        /// </summary>
        private bool _suppressProcessExceptions;

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>> _elementAvailableProperties;

        /// <summary>
        /// Logger
        /// </summary>
        private ILogger<Pipeline> _logger;
        
        /// <summary>
        /// Get a filter that will only include the evidence keys that can 
        /// be used by at least one <see cref="IFlowElement"/> within 
        /// this pipeline.
        /// </summary>
        public IEvidenceKeyFilter EvidenceKeyFilter
        {
            get
            {
                if(_evidenceKeyFilter == null)
                {
                    lock (_evidenceKeyFilterLock)
                    {
                        if (_evidenceKeyFilter == null)
                        {
                            _evidenceKeyFilter = new EvidenceKeyFilterAggregator();
                            foreach (var filter in _flowElements.Select(e => e.EvidenceKeyFilter))
                            {
                                _evidenceKeyFilter.AddFilter(filter);
                            }
                        }
                    }
                }
                return _evidenceKeyFilter;
            }
        }

        /// <summary>
        /// True if multiple <see cref="IFlowElement"/> instances will run 
        /// concurrently within this pipeline. False otherwise.
        /// </summary>
        public bool IsConcurrent
        {
            get
            {
                if (_concurrent.HasValue == false)
                {
                    _concurrent = _flowElements.Any(e => e.IsConcurrent);
                }
                return _concurrent.Value;
            }
        }

        /// <summary>
        /// True if the pipeline has been disposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        /// <summary>
        /// Get a read only list of the flow elements that are part of this 
        /// pipeline.
        /// </summary>
        public IReadOnlyList<IFlowElement> FlowElements
        {
            get
            {
                return new ReadOnlyCollection<IFlowElement>(_flowElements);
            }
        }

        /// <summary>
        /// Get the dictionary of available properties for an
        /// <see cref="IFlowElement.ElementDataKey"/>. The dictionary returned
        /// contains the <see cref="IElementPropertyMetaData"/>s keyed on the
        /// name field.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>> ElementAvailableProperties
        {
            get
            {
                return _elementAvailableProperties;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// Used for logging.
        /// </param>
        /// <param name="flowDataFactory">
        /// Factory method used to create new <see cref="IFlowData"/>
        /// instances.
        /// </param>
        /// <param name="flowElements">
        /// The <see cref="IFlowElement"/> instances that make up this 
        /// pipeline.
        /// </param>
        /// <param name="autoDisposeElements">
        /// If true then Pipeline will call Dispose on it's child elements
        /// when it is disposed.
        /// </param>
        /// <param name="suppressProcessExceptions">
        /// If true then Pipeline will suppress exceptions added to
        /// <see cref="IFlowData.Errors"/>.
        /// </param>
        internal Pipeline(
            ILogger<Pipeline> logger,
            Func<IPipelineInternal, IFlowData> flowDataFactory,
            List<IFlowElement> flowElements,
            bool autoDisposeElements,
            bool suppressProcessExceptions)
        {
            _logger = logger;
            _flowDataFactory = flowDataFactory;
            _flowElements = flowElements;
            _autoDisposeElements = autoDisposeElements;
            _suppressProcessExceptions = suppressProcessExceptions;

            _elementsByType = new Dictionary<Type, List<IFlowElement>>();
            AddElementsByType(_flowElements);

            _elementAvailableProperties = GetElementAvailableProperties(_flowElements);

            _logger.LogInformation($"Pipeline '{GetHashCode()}' created.");
        }

        /// <summary>
        /// Create a new <see cref="IFlowData"/> instance that will use this
        /// pipeline when processing.
        /// </summary>
        /// <returns></returns>
        public IFlowData CreateFlowData()
        {
            return _flowDataFactory(this);
        }

        /// <summary>
        /// Process the given <see cref="IFlowData"/> using the 
        /// <see cref="IFlowElement"/>s in the pipeline.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> that contains the evidence and will
        /// allow the user to access the results.
        /// </param>
        public void Process(IFlowData data)
        {
            _logger.LogDebug($"Pipeline '{GetHashCode()}' started processing.");

            foreach (var element in _flowElements)
            {
                try
                {
                    element.Process(data);
                    if (data.Stop) break;
                }
                catch (Exception ex)
                {
                    // If an error occurs then store it in the 
                    // FlowData object.
                    data.AddError(ex, element);
                }
            }

            // If any errors have occurred and exceptions are not
            // suppressed, then throw an aggregate exception.
            if (data.Errors != null &&
                data.Errors.Count > 0 &&
                _suppressProcessExceptions == false)
            {
                throw new AggregateException(data.Errors
                    .Where(e => e.ShouldThrow == true)
                    .Select(e => e.ExceptionData));
            }

            _logger.LogDebug($"Pipeline '{GetHashCode()}' finished processing.");
        }

        /// <summary>
        /// Get the specified element from the pipeline.
        /// </summary>
        /// <remarks>
        /// If the pipeline contains multiple elements of the requested type,
        /// this method will return null.
        /// </remarks>
        /// <typeparam name="TElement">
        /// The type of the <see cref="IFlowElement"/> to get
        /// </typeparam>
        /// <returns>
        /// An instance of the specified <see cref="IFlowElement"/> if the 
        /// pipeline contains one. 
        /// Null is returned if there is no such instance or there are 
        /// multiple instances of that type.
        /// </returns>
        public TElement GetElement<TElement>()
            where TElement : class, IFlowElement
        {
            TElement result = null;
            List<IFlowElement> elements;
            if (_elementsByType.TryGetValue(typeof(TElement), out elements))
            {
                try
                {
                    result = elements.SingleOrDefault() as TElement;
                }
                catch (InvalidOperationException)
                {
                    result = null;
                }
            }
            else if (_elementsByType.Any(e => typeof(TElement).IsAssignableFrom(e.Key)))
            {
                var matches = _elementsByType.Where(e => typeof(TElement).IsAssignableFrom(e.Key));
                if(matches.Count() == 1 &&
                    matches.Single().Value.Count == 1)
                {
                    result = matches.Single().Value.Single() as TElement;
                }
            }

            return result;
        }

        /// <summary>
        /// Add the specified flow elements to the 
        /// <see cref="_elementsByType"/> dictionary, which contains a list
        /// of all the elements in the pipeline indexed by type.
        /// </summary>
        /// <param name="elements">
        /// The <see cref="IFlowElement"/>s to add.
        /// </param>
        private void AddElementsByType(IReadOnlyList<IFlowElement> elements)
        {
            foreach (var element in elements)
            {
                element.AddPipeline(this);
                var type = element.GetType();
                // If the element is a ParallelElements then add it's child elements.
                if (type == typeof(ParallelElements))
                {
                    AddElementsByType((element as ParallelElements).FlowElements);
                }
                // Otherwise, just add the element directly.
                else
                {
                    List<IFlowElement> typeElements;
                    if (_elementsByType.TryGetValue(type, out typeElements) == false)
                    {
                        typeElements = new List<IFlowElement>();
                        _elementsByType.Add(type, typeElements);
                    }
                    typeElements.Add(element);
                }
            }
        }

        private void AddAvailableProperties(
            IReadOnlyList<IFlowElement> elements,
            IDictionary<string, IDictionary<string, IElementPropertyMetaData>> dictionary)
        {
            foreach (var element in elements)
            {
                if (element is ParallelElements)
                {
                    AddAvailableProperties(((ParallelElements)element).FlowElements, dictionary);
                }
                else
                {
                    if (dictionary.ContainsKey(element.ElementDataKey) == false)
                    {
                        dictionary[element.ElementDataKey] =
                            new Dictionary<string, IElementPropertyMetaData>(
                                StringComparer.OrdinalIgnoreCase);
                    }
                    var availableElementProperties = dictionary[element.ElementDataKey];
                    foreach (var property in element.Properties.Where(p => p.Available))
                    {
                        if (availableElementProperties.ContainsKey(property.Name) == false)
                        {
                            availableElementProperties[property.Name] = property;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Construct the dictionary of available properties for the elements
        /// in the pipeline.
        /// </summary>
        /// <param name="elements">
        /// Elements to get the available properties from
        /// </param>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>
            GetElementAvailableProperties(IReadOnlyList<IFlowElement> elements)
        {
            IDictionary<string, IDictionary<string, IElementPropertyMetaData>> dict =
                new Dictionary<string, IDictionary<string, IElementPropertyMetaData>>(
                    StringComparer.OrdinalIgnoreCase);

            AddAvailableProperties(elements, dict);

            return dict.Select(kvp => new KeyValuePair<string, 
                IReadOnlyDictionary<string, IElementPropertyMetaData>>(
                kvp.Key, (IReadOnlyDictionary<string, IElementPropertyMetaData>)kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Get the meta data for the specified property name.
        /// If there are no properties with that name or multiple 
        /// properties on different elements then an exception will 
        /// be thrown.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to find the meta data for
        /// </param>
        /// <returns>
        /// The meta data associated with the specified property name
        /// </returns>
        /// <exception cref="PipelineDataException">
        /// Thrown if the property name is associated with zero or 
        /// multiple elements.
        /// </exception>
        public IElementPropertyMetaData GetMetaDataForProperty(string propertyName)
        {
            IElementPropertyMetaData result = null;
            if(_metaDataByPropertyName.TryGetValue(propertyName, out result) == false)
            {
                // Get any properties that match the supplied name.
                var properties = FlowElements
                    .SelectMany(e => e.Properties)
                    .Where(p => p.Name.ToLower() == propertyName.ToLower());


                // If there is more than one matching property then log an error.
                if (properties.Count() > 1)
                {
                    string message = $"Multiple matches for property '{propertyName}'. " +
                        $"Flow elements that populate this property are: " +
                        $"'{string.Join(",", properties.Select(p => p.Element.GetType().Name))}'";
                    _logger.LogError(message);
                    throw new PipelineDataException(message);
                }
                // If there are no matching properties then log an error.
                if (properties.Count() == 0)
                {
                    string message = $"Could not find property '{propertyName}'.";
                    _logger.LogError(message);
                    throw new PipelineDataException(message);
                }

                result = properties.Single();
                _metaDataByPropertyName.Add(propertyName, result);
            }

            return result;
        }

        #region IDisposable Support
        /// <summary>
        /// Dispose of any resources.
        /// </summary>
        /// <param name="disposing">
        /// True if Dispose is being called 'correctly' from the Dispose
        /// method.
        /// False if Dispose is being called by the finaliser.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _autoDisposeElements)
                {
                    foreach (var element in _flowElements)
                    {
                        (element as IDisposable).Dispose();
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finaliser
        /// </summary>
        ~Pipeline()
        {
            _logger.LogWarning($"Pipeline '{GetHashCode()}' finalised. It is recommended " +
                $"that instance lifetimes are managed explicitly with a 'using' " +
                $"block or calling the Dispose method as part of a 'finally' block.");
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation($"Pipeline '{GetHashCode()}' disposed.");
            Dispose(true);
            // This line is required to prevent the finaliser above from
            // firing when this instance is finalised.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
