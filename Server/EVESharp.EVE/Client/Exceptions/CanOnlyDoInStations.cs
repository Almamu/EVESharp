using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions;

public class CanOnlyDoInStations : UserError
{
    public CanOnlyDoInStations () : base ("CanOnlyDoInStations") { }
}