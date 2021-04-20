using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChatCustomChannelNameTooLong : UserError
    {
        public ChatCustomChannelNameTooLong(int max) : base("ChatCustomChannelNameTooLong",
            new PyDictionary {["max"] = max})
        {
        }
    }
}