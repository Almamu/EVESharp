using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Packets;

namespace EVESharp.ClusterControler
{
    public class LoginQueueEntry
    {
        public AuthenticationReq request;
        public Connection connection;
    }
}
