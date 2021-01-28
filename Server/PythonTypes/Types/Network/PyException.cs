using System;
using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    /// <summary>
    /// Helper class to work with EVE Online exceptions.
    ///
    /// Can be used in throw statements to ease the communication with the client of these exceptions
    /// </summary>
    public class PyException : Exception
    {
        /// <summary>
        /// The type of exception
        /// </summary>
        public PyToken Type { get; }
        /// <summary>
        /// The reason for the exception, this gives the client extra information on what the exception is all about
        /// </summary>
        public PyString Reason { get; protected set; }
        /// <summary>
        /// Extra information required for the exception
        /// </summary>
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
            if (ex.Header.Count == 2 && ex.Header[1] is PyTuple)
            {
                PyTuple extra = ex.Header[1] as PyTuple;
                
                if(extra.Count == 1)
                    return new PyException(
                        ex.Header[0] as PyToken, ex.Header[1] as PyString, null, null
                    );
                if(extra.Count == 2)
                    return new PyException(
                        ex.Header[0] as PyToken, extra[0] as PyString, extra[1], null
                    );
                
                throw new InvalidDataException("Unexpected amount of arguments for a PyException");   
            }
            if(ex.Header.Count == 3)
            {
                PyTuple extra = ex.Header[1] as PyTuple;
                
                if(extra.Count == 1)
                    return new PyException(
                        ex.Header[0] as PyToken, ex.Header[1] as PyString, null, ex.Header[2] as PyDictionary
                    );
                if(extra.Count == 2)
                    return new PyException(
                        ex.Header[0] as PyToken, extra[0] as PyString, extra[1], ex.Header[2] as PyDictionary
                    );
                
                throw new InvalidDataException("Unexpected amount of arguments for a PyException");
            }
            
            throw new InvalidDataException("Unexpected amount of arguments for a PyException");
        }
    }
}