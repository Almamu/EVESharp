using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions;

public class CanOnlyDoInStations : UserError
{
    public CanOnlyDoInStations() : base("CanOnlyDoInStations")
    {
    }
}