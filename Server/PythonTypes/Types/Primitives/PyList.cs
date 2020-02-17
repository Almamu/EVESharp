using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace PythonTypes.Types.Primitives
{
    public class PyList : PyDataType, IEnumerable<PyDataType>
    {
        private List<PyDataType> mList;
        
        public PyList() : base(PyObjectType.List)
        {
            this.mList = new List<PyDataType>();
        }

        public PyList(int capacity) : base(PyObjectType.List)
        {
            this.mList = new List<PyDataType>(capacity);
        }
        public PyList(PyDataType[] data) : base(PyObjectType.List)
        {
            this.mList = new List<PyDataType>(data);
        }

        public void Add(PyDataType pyDataType)
        {
            this.mList.Add(pyDataType);
        }

        public void Remove(int index)
        {
            this.mList.RemoveAt(index);
        }

        public int Count { get { return this.mList.Count; } }

        public List<PyDataType>.Enumerator GetIterator()
        {
            return this.mList.GetEnumerator();
        }

        public PyDataType this[int index]
        {
            get { return this.mList[index]; }
            set { this.mList[index] = value; }
        }

        public IEnumerator<PyDataType> GetEnumerator()
        {
            return this.mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}