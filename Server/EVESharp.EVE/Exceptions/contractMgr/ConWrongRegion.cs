using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConWrongRegion : UserError
{
    public ConWrongRegion () : base ("ConWrongRegion") { }
}