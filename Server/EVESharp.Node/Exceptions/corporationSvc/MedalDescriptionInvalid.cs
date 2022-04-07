using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

internal class MedalDescriptionInvalid : UserError
{
    public MedalDescriptionInvalid () : base ("MedalDescriptionInvalid") { }
}