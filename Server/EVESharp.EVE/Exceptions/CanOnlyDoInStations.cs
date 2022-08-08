using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions;

public class CanOnlyDoInStations : UserError
{
    public CanOnlyDoInStations () : base ("CanOnlyDoInStations") { }
}