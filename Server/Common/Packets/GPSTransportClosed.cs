using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class GPSTransportClosed : PyException
    {
        private const string TYPE_NAME = "exceptions.GPSTransportClosed";
        
        public GPSTransportClosed(string type) : base(TYPE_NAME, type, new PyDictionary())
        {
            this.reasonArgs = new PyDictionary();
            this.clock = DateTime.Now.ToFileTimeUtc();
            this.region = Common.Constants.Game.region;
            this.Reason = type;
            this.version = Common.Constants.Game.version;
            this.build = Common.Constants.Game.build;
            this.codename = Common.Constants.Game.codename;
            this.machoVersion = Common.Constants.Game.machoVersion;
        }

        public static implicit operator PyDataType(GPSTransportClosed exception)
        {
            exception.Keywords["reasonArgs"] = exception.reasonArgs;
            exception.Keywords["clock"] = exception.clock;
            exception.Keywords["region"] = exception.region;
            exception.Keywords["reason"] = exception.Reason;
            exception.Keywords["version"] = exception.version;
            exception.Keywords["build"] = exception.build;
            exception.Keywords["codename"] = exception.codename;
            exception.Keywords["machoVersion"] = exception.machoVersion;

            return exception as PyException;
        }

        public static implicit operator GPSTransportClosed(PyDataType exception)
        {
            PyException ex = exception;
            
            if(ex.Type != TYPE_NAME)
                throw new Exception($"Expected type {TYPE_NAME} but got {ex.Type}");
            
            GPSTransportClosed result = new GPSTransportClosed(ex.Reason);

            result.Keywords = ex.Keywords;
            
            result.reasonArgs = ex.Keywords["reasonArgs"] as PyDictionary;
            result.clock = ex.Keywords["clock"] as PyInteger;
            result.region = ex.Keywords["region"] as PyString;
            result.Reason = ex.Keywords["reason"] as PyString;
            result.version = ex.Keywords["version"] as PyDecimal;
            result.build = ex.Keywords["build"] as PyInteger;
            result.codename = ex.Keywords["codename"] as PyString;
            result.machoVersion = ex.Keywords["machoVersion"] as PyInteger;
            
            return result;
        }
        
        public string exception_type = "";
        public string message = "";
        public string origin = "";
        public PyDictionary reasonArgs = new PyDictionary();
        public long clock = 0;
        public PyDataType loggedOnUserCount = null;
        public string region = "";
        public double version = 0.0;
        public int build = 0;
        public string reasonCode = "";
        public string codename = "";
        public int machoVersion = 0;
    }
}
