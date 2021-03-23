using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CantTakeInSpaceCapsule : UserError
    {
        public CantTakeInSpaceCapsule() : base("CantTakeInSpaceCapsule")
        {
        }
    }
}