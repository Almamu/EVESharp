using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions;

/// <summary>
/// Helper class to represent simple error messages for the client
/// </summary>
public class CustomInfo : UserError
{
    public CustomInfo(string info) : base("CustomInfo", new PyDictionary {["info"] = info})
    {
    }
}