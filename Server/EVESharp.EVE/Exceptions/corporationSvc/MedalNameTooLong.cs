using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corporationSvc;

public class MedalNameTooLong : UserError
{
    public MedalNameTooLong () : base ("MedalNameTooLong") { }
}