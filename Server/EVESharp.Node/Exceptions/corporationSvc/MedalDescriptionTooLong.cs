using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc;

internal class MedalDescriptionTooLong : UserError
{
    public MedalDescriptionTooLong () : base ("MedalDescriptionTooLong") { }
}