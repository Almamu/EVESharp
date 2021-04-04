using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CannotDestroyFittedItem : UserError
    {
        public CannotDestroyFittedItem() : base("CannotDestroyFittedItem")
        {
        }
    }
}