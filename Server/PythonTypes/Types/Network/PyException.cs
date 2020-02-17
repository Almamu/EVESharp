using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyException
    {
        public string exception_type = "";
        public string message = "";
        public string origin = "";
        public PyDictionary reasonArgs = new PyDictionary();
        public long clock = 0;
        public PyDataType loggedOnUserCount = null;
        public string region = "";
        public string reason = "";
        public double version = 0.0;
        public int build = 0;
        public string reasonCode = "";
        public string codename = "";
        public int machoVersion = 0;

        public static implicit operator PyDataType(PyException exception)
        {
            PyDictionary keywords = new PyDictionary();

            keywords["reasonArgs"] = exception.reasonArgs;
            keywords["clock"] = exception.clock;
            keywords["region"] = exception.region;
            keywords["reason"] = exception.reason;
            keywords["version"] = exception.version;
            keywords["build"] = exception.build;
            keywords["codename"] = exception.codename;
            keywords["machoVersion"] = exception.machoVersion;

            return new PyObject(
                exception.exception_type,
                new PyTuple (new PyDataType[]
                { new PyTuple(new PyDataType[] { exception.reason }) }),
                keywords
            );
        }

        public static implicit operator PyException(PyDataType exception)
        {
            PyException result = new PyException();
            PyObject obj = exception as PyObject;

            result.exception_type = obj.Header.Type;
            result.reason = obj.Header.Arguments[0] as PyString;

            result.origin = obj.Header.Dictionary["origin"] as PyString;
            result.reasonArgs = obj.Header.Dictionary["reasonArgs"] as PyDictionary;
            result.clock = obj.Header.Dictionary["clock"] as PyInteger;
            result.loggedOnUserCount = obj.Header.Dictionary["loggedOnUserCount"] as PyInteger;
            result.region = obj.Header.Dictionary["region"] as PyString;
            result.version = obj.Header.Dictionary["version"] as PyDecimal;
            result.build = obj.Header.Dictionary["build"] as PyInteger;
            result.reasonCode = obj.Header.Dictionary["reasonCode"] as PyString;
            result.codename = obj.Header.Dictionary["codename"] as PyString;
            result.machoVersion = obj.Header.Dictionary["machoVersion"] as PyInteger;

            return result;
        }
    }
}
