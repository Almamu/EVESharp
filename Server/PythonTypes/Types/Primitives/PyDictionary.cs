using System.Collections;
using System.Collections.Generic;

namespace PythonTypes.Types.Primitives
{
    public class PyDictionary : PyDataType, IEnumerable
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

        public IEnumerator GetEnumerator()
        {
            return this.mDictionary.GetEnumerator();
        }
    }
}