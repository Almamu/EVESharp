using System.Collections;
using System.Collections.Generic;

namespace PythonTypes.Types.Primitives
{
    public class PyDictionary : PyDataType, IEnumerable<KeyValuePair<string, PyDataType>>
    {
        private readonly Dictionary<string, PyDataType> mDictionary = new Dictionary<string, PyDataType>();

        public PyDictionary() : base(PyObjectType.List)
        {
        }

        public void Remove(string key)
        {
            this.mDictionary.Remove(key);
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

        public int Length => this.mDictionary.Count;

        public IEnumerator<KeyValuePair<string, PyDataType>> GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}