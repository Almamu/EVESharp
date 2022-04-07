using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

internal class MedalNameInvalid : UserError
{
    public MedalNameInvalid () : base ("MedalNameInvalid") { }
}