using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Collections
{
    [Serializable]
    public struct LazyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _items;

        private Dictionary<TKey, TValue> Items {
            get {
                if (!HasItems)
                    _items = new Dictionary<TKey, TValue>();
                return _items;
            }
        }

        private bool HasItems
        {
            get { return _items!=null; }
        }

        #region Implementation of IDictionary<TKey,TValue>

        public TValue this[TKey key] {
            get {
                if (!HasItems)
                    throw new KeyNotFoundException();
                return _items[key];
            }
            set { Items[key] = value; }
        }

        public ICollection<TKey> Keys {
            get {
                return HasItems ? (ICollection<TKey>) _items.Keys : new List<TKey>();
            }
        }

        public ICollection<TValue> Values {
            get {
                return HasItems ? (ICollection<TValue>) _items.Values : new List<TValue>();
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (!HasItems)
                return false;
            return _items.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            Items.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            if (!HasItems)
                return false;
            return _items.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!HasItems) {
                value = default(TValue);
                return false;
            }
            return _items.TryGetValue(key, out value);
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        public int Count {
            get { return HasItems ? _items.Count : 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Clear()
        {
            _items = null;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Items.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!HasItems)
                return false;
            return _items.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (!HasItems)
                return;
            (_items as IDictionary<TKey, TValue>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!HasItems)
                return false;
            return (_items as IDictionary<TKey, TValue>).Remove(item);
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return HasItems 
                ? (_items as IEnumerable<KeyValuePair<TKey, TValue>>).GetEnumerator() 
                : EnumerableUtils<KeyValuePair<TKey, TValue>>.Empty.GetEnumerator();
        }

        #endregion
    }

}
