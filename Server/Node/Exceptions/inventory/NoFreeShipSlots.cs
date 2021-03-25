using Node.Inventory.Items.Types;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class NoFreeShipSlots : UserError
    {
        public NoFreeShipSlots() : base("NoFreeShipSlots")
        {
            
        }
    }
}