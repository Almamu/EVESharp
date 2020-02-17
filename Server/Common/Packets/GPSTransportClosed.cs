using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Network;

namespace Common.Packets
{
    public class GPSTransportClosed : PyException
    {
        public GPSTransportClosed(string type)
        {
            exception_type = "exceptions.GPSTransportClosed";
            reason = type;

            region = Constants.Game.region;
            codename = Constants.Game.codename;
            machoVersion = Constants.Game.machoVersion;
            version = Constants.Game.version;
            build = Constants.Game.build;
            clock = DateTime.Now.ToFileTimeUtc();
        }
    }
}
