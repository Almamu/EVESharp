using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corporationSvc;

public class MedalNameInvalid : UserError
{
    public MedalNameInvalid () : base ("MedalNameInvalid") { }
}