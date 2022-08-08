using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.slash;

public class SlashError : UserError
{
    public SlashError (string reason) : base ("SlashError", new PyDictionary {["reason"] = reason}) { }
}