using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CantFitToCapsule : UserError
    {
        public CantFitToCapsule() : base("CantFitToCapsule")
        {
        }
    }
}