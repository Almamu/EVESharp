using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.jumpCloneSvc;

public class MktNotOwner : UserError
{
    public MktNotOwner () : base ("MktNotOwner") { }
}