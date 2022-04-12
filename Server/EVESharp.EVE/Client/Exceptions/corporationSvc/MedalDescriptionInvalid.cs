using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corporationSvc;

public class MedalDescriptionInvalid : UserError
{
    public MedalDescriptionInvalid () : base ("MedalDescriptionInvalid") { }
}