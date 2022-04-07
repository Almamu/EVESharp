using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

class MedalNameInvalid : UserError
{
    public MedalNameInvalid() : base("MedalNameInvalid")
    {
    }
}