using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConDurationZero : UserError
{
    public ConDurationZero() : base("ConDurationZero")
    {
    }
}