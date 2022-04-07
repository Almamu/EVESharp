using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.jumpCloneSvc;

public class MktNotOwner : UserError
{
    public MktNotOwner() : base("MktNotOwner")
    {
    }
}