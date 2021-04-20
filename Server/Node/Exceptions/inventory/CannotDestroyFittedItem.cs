using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class CannotDestroyFittedItem : UserError
    {
        public CannotDestroyFittedItem() : base("CannotDestroyFittedItem")
        {
        }
    }
}