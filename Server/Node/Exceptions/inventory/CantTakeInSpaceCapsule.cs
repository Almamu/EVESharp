using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class CantTakeInSpaceCapsule : UserError
    {
        public CantTakeInSpaceCapsule() : base("CantTakeInSpaceCapsule")
        {
        }
    }
}