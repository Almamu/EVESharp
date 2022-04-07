using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy;

public class MktOrderDidNotMatch : UserError
{
    public MktOrderDidNotMatch() : base("MktOrderDidNotMatch")
    {
    }
}