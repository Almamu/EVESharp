using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions
{
    public class ChatCustomChannelNameTooLong : UserError
    {
        public ChatCustomChannelNameTooLong(int max) : base("ChatCustomChannelNameTooLong",
            new PyDictionary {["max"] = max})
        {
        }
    }
}