using System;
using System.Collections;
using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public class PyList<T> : PyList, IPyListEnumerable<T> where T : PyDataType
    {
        public PyList()
        {
        }

        public PyList(int capacity) : base(capacity)
        {
        }

        public PyList(PyDataType[] data) : base(data)
        {
        }

        public PyList(List<PyDataType> seed) : base(seed)
        {
        }

        public void Add(T value)
        {
            base.Add(value);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new IPyListEnumerator<T> GetEnumerator()
        {
            return new PyListEnumerator<T>(this.mList.GetEnumerator());
        }

        public new T this[int index]
        {
            get => base[index] as T;
            set => base[index] = value;
        }

        public static implicit operator PyList<T>(PyDataType[] array)
        {
            return new PyList<T>(array);
        }

        public static implicit operator object[](PyList<T> original)
        {
            throw new NotSupportedException("This exception means that most likely what you're trying to achieve is not being done automatically by the compiler, please check the method you're passing this object to to ensure you get the real IEnumerable and not a different overload");
        }
    }
    
    public class PyList : PyDataType, IPyListEnumerable<PyDataType>
    {
        protected readonly List<PyDataType> mList;
        public PyList()
        {
            this.mList = new List<PyDataType>();
        }

        public PyList(int capacity)
        {
            this.mList = new List<PyDataType>(new PyDataType[capacity]);
        }

        public PyList(PyDataType[] data)
        {
            this.mList = new List<PyDataType>(data);
        }

        public PyList(List<PyDataType> seed)
        {
            this.mList = seed;
        }

        public void Add(PyDataType pyDataType)
        {
            this.mList.Add(pyDataType);
        }

        public void AddRange(IEnumerable<PyDataType> list)
        {
            this.mList.AddRange(list);
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

        IEnumerator<PyDataType> IEnumerable<PyDataType>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPyListEnumerator<PyDataType> GetEnumerator()
        {
            return new PyListEnumerator<PyDataType>(this.mList.GetEnumerator());
        }
        
        public PyList<T> GetEnumerable<T>() where T : PyDataType
        {
            return new PyList<T>(this.mList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}