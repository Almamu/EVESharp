using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class CantFitToCapsule : UserError
    {
        public CantFitToCapsule() : base("CantFitToCapsule")
        {
        }
    }
}