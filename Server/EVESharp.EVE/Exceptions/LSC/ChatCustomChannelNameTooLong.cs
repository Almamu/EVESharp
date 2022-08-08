using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.LSC;

public class ChatCustomChannelNameTooLong : UserError
{
    public ChatCustomChannelNameTooLong (int max) : base (
        "ChatCustomChannelNameTooLong",
        new PyDictionary {["max"] = max}
    ) { }
}