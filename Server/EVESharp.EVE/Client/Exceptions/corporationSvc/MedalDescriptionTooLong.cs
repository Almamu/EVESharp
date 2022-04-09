using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corporationSvc;

public class MedalDescriptionTooLong : UserError
{
    public MedalDescriptionTooLong () : base ("MedalDescriptionTooLong") { }
}