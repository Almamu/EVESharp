using EVE.Packets.Exceptions;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class NoFreeShipSlots : UserError
    {
        public NoFreeShipSlots() : base("NoFreeShipSlots")
        {
        }
    }
}