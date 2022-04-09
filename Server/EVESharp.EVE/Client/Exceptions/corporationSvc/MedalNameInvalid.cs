using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corporationSvc;

public class MedalNameInvalid : UserError
{
    public MedalNameInvalid () : base ("MedalNameInvalid") { }
}