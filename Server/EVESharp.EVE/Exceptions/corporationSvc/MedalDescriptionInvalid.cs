using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corporationSvc;

public class MedalDescriptionInvalid : UserError
{
    public MedalDescriptionInvalid () : base ("MedalDescriptionInvalid") { }
}