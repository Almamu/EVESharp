using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace PythonTypes.Types.Primitives
{
    public class PyDictionary : PyDataType, IEnumerable<KeyValuePair<string, PyDataType>>
    {
        private Dictionary<string, PyDataType> mDictionary = new Dictionary<string, PyDataType>();
        
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
            get { return this.mDictionary[key]; }
            set { this.mDictionary[key] = value; }
        }

        public int Length
        {
            get { return this.mDictionary.Count; }
        }
        
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