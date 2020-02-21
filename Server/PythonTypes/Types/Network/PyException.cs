using System;
using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyException : Exception
    {
        public PyToken Type { get; }
        public PyString Reason { get; protected set; }
        public PyDictionary Keywords { get; protected set; }
        public PyDataType Extra { get; }

        public PyException(string type, string reason, PyDataType extra, PyDictionary keywords)
        {
            this.Type = type;
            this.Reason = reason;
            this.Extra = extra;
            this.Keywords = keywords;
        }

        public static implicit operator PyDataType(PyException ex)
        {
            PyTuple data = new PyTuple(ex.Extra == null ? 1 : 2);

            data[0] = ex.Reason;

            if (ex.Extra != null)
                data[1] = ex.Extra;
            
            return new PyObject(
                false,
                new PyTuple(new PyDataType[] {ex.Type, data, ex.Keywords})
            );
        }

        public static implicit operator PyException(PyDataType exception)
        {
            if (exception is PyObject == false)
                throw new Exception("Expected object");

            PyObject ex = exception as PyObject;

            if(ex.Header.Count == 1)
                return new PyException(
                    ex.Header[0] as PyToken, "", null, null
                );
            else if (ex.Header.Count == 2 && ex.Header[1] is PyTuple)
            {
                PyTuple extra = ex.Header[1] as PyTuple;
                
                if(extra.Count == 1)
                    return new PyException(
                        ex.Header[0] as PyToken, ex.Header[1] as PyString, null, null
                    );
                else if(extra.Count == 2)
                    return new PyException(
                        ex.Header[0] as PyToken, extra[0] as PyString, extra[1], null
                    );
                else
                    throw new InvalidDataException("Unexpected amount of arguments for a PyException");   
            }
            else if(ex.Header.Count == 3)
            {
                PyTuple extra = ex.Header[1] as PyTuple;
                
                if(extra.Count == 1)
                    return new PyException(
                        ex.Header[0] as PyToken, ex.Header[1] as PyString, null, ex.Header[2] as PyDictionary
                    );
                else if(extra.Count == 2)
                    return new PyException(
                        ex.Header[0] as PyToken, extra[0] as PyString, extra[1], ex.Header[2] as PyDictionary
                    );
                else
                    throw new InvalidDataException("Unexpected amount of arguments for a PyException");
            }
            else
            {
                throw new InvalidDataException("Unexpected amount of arguments for a PyException");
            }
        }
    }
}