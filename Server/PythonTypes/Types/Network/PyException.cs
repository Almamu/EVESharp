using System;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyException : Exception
    {
        public PyToken Type { get; }
        public PyString Reason { get; protected set; }
        public PyDictionary Keywords { get; protected set; }

        public PyException(string type, string reason, PyDictionary keywords)
        {
            this.Type = type;
            this.Reason = reason;
            this.Keywords = keywords;
        }

        public static implicit operator PyDataType(PyException ex)
        {
            return new PyObject(
                ex.Type,
                new PyTuple (new PyDataType[] { new PyTuple ( new PyDataType[] { new PyTuple(new PyDataType[] { ex.Reason }) }) }),
                ex.Keywords
            );
        }

        public static implicit operator PyException(PyDataType exception)
        {
            if (exception is PyObject == false)
                throw new Exception("Expected object");

            PyObject ex = exception as PyObject;

            return new PyException(
                ex.Header.Type, ex.Header.Arguments[0] as PyString, ex.Header.Dictionary
            );
        }
    }
}