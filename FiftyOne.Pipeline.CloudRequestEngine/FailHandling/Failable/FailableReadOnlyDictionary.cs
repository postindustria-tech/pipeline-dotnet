using FiftyOne.Pipeline.Core.Data;
using System.Collections;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Failable
{
    internal class FailableReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V>, IFailableLazyResult
    {
        private readonly IReadOnlyDictionary<K, V> _readOnlyDictionary;
        private readonly bool _mayBeSaved;

        public FailableReadOnlyDictionary(IReadOnlyDictionary<K, V> readOnlyDictionary, bool mayBeSaved)
        {
            _readOnlyDictionary = readOnlyDictionary;
            _mayBeSaved = mayBeSaved;
        }

        public bool MayBeSaved => _mayBeSaved;

        public V this[K key] => _readOnlyDictionary[key];
        public IEnumerable<K> Keys => _readOnlyDictionary.Keys;
        public IEnumerable<V> Values => _readOnlyDictionary.Values;
        public int Count => _readOnlyDictionary.Count;
        public bool ContainsKey(K key) => _readOnlyDictionary.ContainsKey(key);
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            => _readOnlyDictionary.GetEnumerator();
        public bool TryGetValue(K key, out V value)
            => _readOnlyDictionary.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _readOnlyDictionary.GetEnumerator();
    }
}
