using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public class PyTuple : PyDataType, IEnumerable<PyDataType>
    {
        private readonly PyDataType[] mList;

        public PyTuple(int size) : base()
        {
            this.mList = new PyDataType[size];
        }

        public PyTuple(PyDataType[] data) : base()
        {
            this.mList = data;
        }

        // THESE CONSTRUCTORS ARE HERE TO PREVENT ERRORS WHEN USING THE TYPE-GUESSING STUFF IN C#
        // when you use new PyTuple (new [] { new PyNone(), new PyNone() }) C# creates a new array of PyNones
        // but as we're using the array directly in the PyTuple if we try to add something that's not a PyNone
        // an exception will be thrown
        // these exceptions are launched to prevent this situation
        public PyTuple(PyString[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyDecimal[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyList[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyTuple[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyInteger[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PySubStream[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyPackedRow[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyChecksumedStream[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyBuffer[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyBool[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyObject[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyObjectData[] data) : base() { ThrowInstantiationError(); }
        public PyTuple(PyToken[] data) : base() { ThrowInstantiationError(); }

        private static void ThrowInstantiationError()
        {
            throw new InvalidDataException(
                "when you use new PyTuple (new [] { new PyNone(), new PyNone() }) C# creates a new array of PyNones" +
                Environment.NewLine +
                "but as the array is used directly in the PyTuple if we try to add something that's not a PyNone" +
                Environment.NewLine +
                "an exception will be thrown" + Environment.NewLine +
                "this exception is thrown to prevent and explain this situation" + Environment.NewLine +
                "please use the new PyTypeData[] { new PyNone(), new PyNone() } form instead to get a proper array"
            );
        }

        public PyDataType this[int index]
        {
            get => this.mList[index];
            set => this.mList[index] = value;
        }

        public int Count => this.mList.Length;

        public IEnumerator<PyDataType> GetEnumerator()
        {
            return ((IEnumerable<PyDataType>) this.mList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetValue<T>(int key, out T value) where T : PyDataType
        {
            if (key < this.mList.Length)
            {
                value = this.mList[key] as T;
                return true;
            }

            value = null;
            return false;
        }

        public void CopyTo(PyTuple destination, int sourceIndex, int destinationIndex, int count)
        {
            // perform some boundaries checks to ensure the data fits
            if (
                (count + destinationIndex - sourceIndex) > destination.Count ||
                sourceIndex > this.Count ||
                sourceIndex + count > this.Count ||
                sourceIndex < 0 ||
                destinationIndex < 0)
                throw new IndexOutOfRangeException("Trying to copy tuple items that would be out of range");

            // copy data over
            Array.Copy(
                this.mList, sourceIndex,
                destination.mList, destinationIndex, count
            );
        }

        public static implicit operator PyTuple(PyDataType[] array)
        {
            return new PyTuple(array);
        }
    }
}