using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PythonTypes.Types.Database;

namespace PythonTypes.Types.Primitives
{
    public class PyTuple : PyDataType, IEnumerable<PyDataType>
    {
        private PyDataType[] mList;
        
        public PyTuple(int size) : base(PyObjectType.Tuple)
        {
            this.mList = new PyDataType[size];
        }

        public PyTuple(PyDataType[] data) : base(PyObjectType.Tuple)
        {
            this.mList = data;
        }
        
        // THESE CONSTRUCTORS ARE HERE TO PREVENT ERRORS WHEN USING THE TYPE-GUESSING STUFF IN C#
        // when you use new PyTuple (new [] { new PyNone(), new PyNone() }) C# creates a new array of PyNones
        // but as we're using the array directly in the PyTuple if we try to add something that's not a PyNone
        // an exception will be thrown
        // these exceptions are launched to prevent this situation
        public PyTuple(PyNone[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyString[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyDecimal[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyList[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyTuple[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyInteger[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PySubStream[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyPackedRow[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyChecksumedStream[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyBuffer[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyBool[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyObject[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyObjectData[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }
        public PyTuple(PyToken[] data) : base(PyObjectType.Tuple) { ThrowInstantiationError(); }

        private static void ThrowInstantiationError()
        {
            throw new InvalidDataException(
                "when you use new PyTuple (new [] { new PyNone(), new PyNone() }) C# creates a new array of PyNones" + Environment.NewLine + 
                "but as the array is used directly in the PyTuple if we try to add something that's not a PyNone" + Environment.NewLine +
                "an exception will be thrown" + Environment.NewLine +
                "this exception is thrown to prevent and explain this situation" + Environment.NewLine +
                "please use the new PyTypeData[] { new PyNone(), new PyNone() } form instead to get a proper array"
            );
        }

        public PyDataType this[int index]
        {
            get { return this.mList[index]; }
            set { this.mList[index] = value; }
        }
        
        public int Count { get { return this.mList.Length; } }
        public IEnumerator<PyDataType> GetEnumerator()
        {
            return ((IEnumerable<PyDataType>)this.mList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(PyTuple destination, int sourceIndex, int destinationIndex)
        {
            // perform some boundaries checks to ensure the data fits
            if(
                (this.Count + destinationIndex - sourceIndex) > destination.Count ||
                sourceIndex > this.Count ||
                sourceIndex < 0 ||
                destinationIndex < 0)
                throw new IndexOutOfRangeException("Trying to copy tuple items that would be out of range");

            // copy data over
            Array.Copy(
                this.mList, sourceIndex,
                destination.mList, destinationIndex, this.Count - sourceIndex
            );
        }
    }
}