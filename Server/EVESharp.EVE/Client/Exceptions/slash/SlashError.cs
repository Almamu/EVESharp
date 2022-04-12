using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.slash;

public class SlashError : UserError
{
    public SlashError (string reason) : base ("SlashError", new PyDictionary {["reason"] = reason}) { }
}