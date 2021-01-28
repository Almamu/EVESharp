using PythonTypes.Types.Exceptions;

namespace Node.Exceptions
{
    public class CanOnlyDoInStations : UserError
    {
        public CanOnlyDoInStations() : base("CanOnlyDoInStations")
        {
        }
    }
}