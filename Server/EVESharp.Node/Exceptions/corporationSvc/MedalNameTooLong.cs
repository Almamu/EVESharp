using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

class MedalNameTooLong : UserError
{
    public MedalNameTooLong() : base("MedalNameTooLong")
    {
    }
}