using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

internal class MedalNameTooLong : UserError
{
    public MedalNameTooLong () : base ("MedalNameTooLong") { }
}