using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConDurationZero : UserError
{
    public ConDurationZero () : base ("ConDurationZero") { }
}