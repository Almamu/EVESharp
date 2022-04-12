using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corporationSvc;

public class MedalNameTooLong : UserError
{
    public MedalNameTooLong () : base ("MedalNameTooLong") { }
}