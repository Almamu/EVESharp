#region

using System.Collections;
using System.Collections.Generic;

#endregion

namespace PythonTypes.Types.Primitives
{
    public class PyList : PyDataType, IEnumerable<PyDataType>
    {
        private readonly List<PyDataType> mList;

        public PyList() : base(PyObjectType.List)
        {
            this.mList = new List<PyDataType>();
        }

        public PyList(int capacity) : base(PyObjectType.List)
        {
            this.mList = new List<PyDataType>(new PyDataType[capacity]);
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

        public int Count => this.mList.Count;

        public List<PyDataType>.Enumerator GetIterator()
        {
            return this.mList.GetEnumerator();
        }

        public PyDataType this[int index]
        {
            get => this.mList[index];
            set => this.mList[index] = value;
        }

        public IEnumerator<PyDataType> GetEnumerator()
        {
            return this.mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator PyList(PyDataType[] array)
        {
            return new PyList(array);
        }

        public PyTuple AsTuple()
        {
            return new PyTuple(this.mList.ToArray());
        }
    }
}