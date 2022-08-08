using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.jumpCloneSvc;

public class MktNotOwner : UserError
{
    public MktNotOwner () : base ("MktNotOwner") { }
}