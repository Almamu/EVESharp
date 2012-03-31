using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marshal
{
    static public class Macho
    {
        public enum MachoNetMsg_Type
        {
            AUTHENTICATION_REQ = 0,
            AUTHENTICATION_RSP = 1,
            IDENTIFICATION_REQ = 2,
            IDENTIFICATION_RSP = 3,
            __Fake_Invalid_Type = 4,
            CALL_REQ = 6,
            CALL_RSP = 7,
            TRANSPORTCLOSED = 8,
            RESOLVE_REQ = 10,
            RESOLVE_RSP = 11,
            NOTIFICATION = 12,
            ERRORRESPONSE = 15,
            SESSIONCHANGENOTIFICATION = 16,
            SESSIONINITIALSTATENOTIFICATION = 18,
            PING_REQ = 20,
            PING_RSP = 21,
            MACHONETMSG_TYPE_COUNT
        }
    }
}
