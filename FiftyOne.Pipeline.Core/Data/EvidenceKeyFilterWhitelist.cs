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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Core.Data
{
    /// <summary>
    /// This evidence filter will only include keys that are on a whitelist
    /// that is specified at construction time.
    /// </summary>
    public class EvidenceKeyFilterWhitelist : IEvidenceKeyFilter
    {
        /// <summary>
        /// The dictionary containing all keys in the whitelist and the
        /// order of precedence.
        /// </summary>
        protected Dictionary<string, int> _whitelist;
        /// <summary>
        /// The equality comparer that is used to determine if a supplied
        /// string key is in the whitelist or not.
        /// </summary>
        protected IEqualityComparer<string> _comparer;

        /// <summary>
        /// Get the keys in the white list as a read only dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, int> Whitelist
        {
            get
            {
                return new ReadOnlyDictionary<string, int>(_whitelist);
            }
        }

        /// <summary>
        /// Get the equality comparer that is used to determine if a supplied
        /// string key is in the whitelist or not.
        /// </summary>
        public IEqualityComparer<string> Comparer
        {
            get { return _comparer; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whitelist">
        /// The list of evidence keys that is filter will include.
        /// By default, all keys will have the same order of precedence.
        /// </param>
        public EvidenceKeyFilterWhitelist(List<string> whitelist)
        {
            _whitelist = whitelist.ToDictionary(w => w, w => 0);
            _comparer = EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whitelist">
        /// The list of evidence keys that is filter will include.
        /// By default, all keys will have the same order of precedence.
        /// </param>
        /// <param name="comparer">
        /// Comparator to use when comparing the keys.
        /// </param>
        public EvidenceKeyFilterWhitelist(
            List<string> whitelist,
            IEqualityComparer<string> comparer)
        {
            _whitelist = whitelist.ToDictionary(w => w, w => 0, comparer);
            _comparer = comparer;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whitelist">
        /// The dictionary of evidence keys that is filter will include.
        /// The order of precedence of each key is given by the value of
        /// the key/value pair.
        /// </param>
        public EvidenceKeyFilterWhitelist(Dictionary<string, int> whitelist)
        {
            _whitelist = whitelist.ToDictionary(w => w.Key, w => w.Value);
            _comparer = EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whitelist">
        /// The dictionary of evidence keys that is filter will include.
        /// The order of precedence of each key is given by the value of
        /// the key/value pair.
        /// </param>
        /// <param name="comparer">
        /// Comparator to use when comparing the keys.
        /// </param>
        public EvidenceKeyFilterWhitelist(
            Dictionary<string, int> whitelist,
            IEqualityComparer<string> comparer)
        {
            _whitelist = whitelist.ToDictionary(w => w.Key, w => w.Value, comparer);
            _comparer = comparer;
        }

        /// <summary>
        /// Check if the specified evidence key is included by this filter.
        /// </summary>
        /// <param name="key">
        /// The key to check
        /// </param>
        /// <returns>
        /// True if the key is included and false if not.
        /// </returns>
        public virtual bool Include(string key)
        {
            return _whitelist.ContainsKey(key);
        }

        /// <summary>
        /// Get the order of precedence of the specified key
        /// </summary>
        /// <param name="key">
        /// The key to check
        /// </param>
        /// <returns>
        /// The order, where lower values indicate a higher order of 
        /// precedence. 
        /// Null if the key is not in the white list.
        /// </returns>
        public virtual int? Order(string key)
        {
            int? result = 0;
            int temp;
            if(_whitelist.TryGetValue(key, out temp) == false)
            {
                result = null;
            }
            else
            {
                result = temp;
            }
            return result;
        }
    }
}
