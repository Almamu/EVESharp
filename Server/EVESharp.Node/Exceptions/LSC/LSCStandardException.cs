using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions;

public class LSCStandardException : UserError
{
    public LSCStandardException(string type, string message) : base(type, new PyDictionary {["msg"] = message})
    {
    }
}