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

using FiftyOne.Caching;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Engines.Caching
{
    /// <summary>
    /// Abstract base class for caches that use <see cref="IFlowData"/> as 
    /// the key.
    /// Internally, the cache actually uses a <see cref="DataKey"/> instance
    /// derived from <see cref="IFlowData"/>.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of data to store in the cache.
    /// </typeparam>
    public abstract class DataKeyedCacheBase<TValue> : IDataKeyedCache<TValue>
    {
        /// <summary>
        /// The cache that is actually used to store the data internally.
        /// </summary>
        private IPutCache<DataKey, TValue> _internalCache;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">
        /// The cache configuration to use when creating the internal cache.
        /// </param>
        public DataKeyedCacheBase(CacheConfiguration configuration)
        {
            _internalCache = configuration.Builder.Build<DataKey, TValue>(configuration.Size) 
                as IPutCache<DataKey, TValue>;
            if(_internalCache == null)
            {
                throw new Exception(
                    $"Cache builder '{configuration.Builder.GetType().Name}' " +
                    $"does not produce caches conforming to 'IPutCache'");
            }
        }

        /// <summary>
        /// Get the <see cref="TValue"/> associated with the key generated 
        /// from the supplied <see cref="IFlowData"/> instance.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> to use as a key.
        /// </param>
        /// <returns>
        /// If a matching item exists in the cache then the 
        /// <see cref="TValue"/> is returned. If not, the default value
        /// is returned. (i.e. null for reference types, 0 for int, etc) 
        /// </returns>
        public virtual TValue this[IFlowData data]
        {
           get { return _internalCache[data.GenerateKey(GetFilter())]; }
        }

        /// <summary>
        /// Add the specified data to the cache.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> to use as a key.
        /// </param>
        /// <param name="value">
        /// The <see cref="TValue"/> to store with the key.
        /// </param>
        public virtual void Put(IFlowData data, TValue value)
        {
            _internalCache.Put(data.GenerateKey(GetFilter()), value);
        }

        /// <summary>
        /// Returns the <see cref="IEvidenceKeyFilter"/> to use when 
        /// generating a key from <see cref="IFlowData"/> instances.
        /// Only evidence values that the filter includes will be used to 
        /// create the key.
        /// </summary>
        /// <returns>
        /// An <see cref="IEvidenceKeyFilter"/> instance.
        /// </returns>
        protected abstract IEvidenceKeyFilter GetFilter();

        /// <summary>
        /// IDisposable support
        /// </summary>
        public void Dispose()
        {
            _internalCache.Dispose();
        }
    }
}
