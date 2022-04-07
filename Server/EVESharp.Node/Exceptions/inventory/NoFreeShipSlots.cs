using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.inventory;

public class NoFreeShipSlots : UserError
{
    public NoFreeShipSlots() : base("NoFreeShipSlots")
    {
    }
}