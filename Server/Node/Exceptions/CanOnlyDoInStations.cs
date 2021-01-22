using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class CanOnlyDoInStations : UserError
    {
        public CanOnlyDoInStations() : base("CanOnlyDoInStations")
        {
        }
    }
}