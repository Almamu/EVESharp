#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace PythonTypes.Types.Primitives
{
    public interface IPyListEnumerable<T> : IEnumerable where T : PyDataType
    {
        new IPyListEnumerator<T> GetEnumerator();
    }

    public interface IPyListEnumerator<T> : IEnumerator<T> where T : PyDataType
    {
    }
    
    public class PyListEnumerator<T> : IPyListEnumerator<T> where T : PyDataType
    {
        protected IEnumerator<PyDataType> mEnumerator;

        public PyListEnumerator(IEnumerator<PyDataType> enumerator)
        {
            this.mEnumerator = enumerator;
        }
        
        public bool MoveNext()
        {
            return this.mEnumerator.MoveNext();
        }

        public void Reset()
        {
            this.mEnumerator.Reset();
        }

        public T Current => this.mEnumerator.Current as T;

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            this.mEnumerator.Dispose();
        }
    }
    
    public class PyList<T> : PyList, IPyListEnumerable<T> where T : PyDataType
    {
        public PyList() : base()
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

        public new void Add(T value)
        {
            base.Add(value);
        }

        public IPyListEnumerator<T> GetEnumerator()
        {
            return new PyListEnumerator<T>(this.mList.GetEnumerator());
        }
    }
    
    public class PyList : PyDataType, IPyListEnumerable<PyDataType>
    {
        protected readonly List<PyDataType> mList;
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

        public PyList(List<PyDataType> seed) : base(PyObjectType.List)
        {
            this.mList = seed;
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