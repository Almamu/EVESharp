using EVE.Packets.Exceptions;

namespace Node.Exceptions
{
    public class CanOnlyDoInStations : UserError
    {
        public CanOnlyDoInStations() : base("CanOnlyDoInStations")
        {
        }
    }
}