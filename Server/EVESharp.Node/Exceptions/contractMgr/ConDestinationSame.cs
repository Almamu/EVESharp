using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConDestinationSame : UserError
{
    public ConDestinationSame() : base("ConDestinationSame")
    {
    }
}