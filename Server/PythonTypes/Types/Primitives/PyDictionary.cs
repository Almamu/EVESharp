using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PythonTypes.Types.Primitives
{
    public class PyDictionary : PyDataType, IDictionary<string, PyDataType>
    {
        private readonly Dictionary<string, PyDataType> mDictionary;

        public PyDictionary() : base(PyObjectType.Dictionary)
        {
            this.mDictionary = new Dictionary<string, PyDataType>();
        }

        public PyDictionary(Dictionary<string, PyDataType> seed) : base(PyObjectType.Dictionary)
        {
            this.mDictionary = seed;
        }

        public bool ContainsKey(string key)
        {
            return this.mDictionary.ContainsKey(key);
        }

        public PyDataType this[string key]
        {
            get => this.mDictionary[key];
            set => this.mDictionary[key] = value;
        }

        public static implicit operator PyDictionary(Dictionary<string, PyDataType> value)
        {
            return new PyDictionary(value);
        }

        public bool TryGetValue(string key, out PyDataType value)
        {
            return this.mDictionary.TryGetValue(key, out value);
        }

        public void Add(string key, PyDataType value)
        {
            this.mDictionary.Add(key, value);
        }
        
        public ICollection<string> Keys => this.mDictionary.Keys;
        public ICollection<PyDataType> Values => this.mDictionary.Values;

        public int Length => this.mDictionary.Count;

        IEnumerator<KeyValuePair<string, PyDataType>> IEnumerable<KeyValuePair<string, PyDataType>>.GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }
        
        public IEnumerator GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<string, PyDataType> item)
        {
            this.mDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.mDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, PyDataType> item)
        {
            return this.mDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, PyDataType>[] array, int arrayIndex)
        {
            // not like this function is needed anyway...
            throw new NotImplementedException();
        }
        
        public bool Remove(string key)
        {
            return this.mDictionary.Remove(key);
        }
        
        public bool Remove(KeyValuePair<string, PyDataType> item)
        {
            return this.mDictionary.Remove(item.Key);
        }

        public int Count => this.mDictionary.Count;
        public bool IsReadOnly => false;
    }
}