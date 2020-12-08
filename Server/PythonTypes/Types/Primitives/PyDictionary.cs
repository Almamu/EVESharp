using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PythonTypes.Types.Primitives
{
    public class PyDictionary : PyDataType, IDictionary<PyDataType, PyDataType>
    {
        private readonly Dictionary<PyDataType, PyDataType> mDictionary;

        public PyDictionary() : base(PyObjectType.Dictionary)
        {
            this.mDictionary = new Dictionary<PyDataType, PyDataType>();
        }

        public PyDictionary(Dictionary<PyDataType, PyDataType> seed) : base(PyObjectType.Dictionary)
        {
            this.mDictionary = seed;
        }

        public bool ContainsKey(PyDataType key)
        {
            return this.mDictionary.ContainsKey(key);
        }

        public PyDataType this[PyDataType key]
        {
            get => this.mDictionary[key];
            set => this.mDictionary[key] = value;
        }

        public static implicit operator PyDictionary(Dictionary<PyDataType, PyDataType> value)
        {
            return new PyDictionary(value);
        }

        public bool TryGetValue(PyDataType key, out PyDataType value)
        {
            return this.mDictionary.TryGetValue(key, out value);
        }

        public void Add(PyDataType key, PyDataType value)
        {
            this.mDictionary.Add(key, value);
        }
        
        public ICollection<PyDataType> Keys => this.mDictionary.Keys;
        public ICollection<PyDataType> Values => this.mDictionary.Values;

        public int Length => this.mDictionary.Count;

        IEnumerator<KeyValuePair<PyDataType, PyDataType>> IEnumerable<KeyValuePair<PyDataType, PyDataType>>.GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }
        
        public IEnumerator GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<PyDataType, PyDataType> item)
        {
            this.mDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.mDictionary.Clear();
        }

        public bool Contains(KeyValuePair<PyDataType, PyDataType> item)
        {
            return this.mDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<PyDataType, PyDataType>[] array, int arrayIndex)
        {
            // not like this function is needed anyway...
            throw new NotImplementedException();
        }
        
        public bool Remove(PyDataType key)
        {
            return this.mDictionary.Remove(key);
        }
        
        public bool Remove(KeyValuePair<PyDataType, PyDataType> item)
        {
            return this.mDictionary.Remove(item.Key);
        }

        public int Count => this.mDictionary.Count;
        public bool IsReadOnly => false;
    }
}