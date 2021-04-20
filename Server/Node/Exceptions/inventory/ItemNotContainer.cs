using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class ItemNotContainer : UserError
    {
        public ItemNotContainer(string itemInfo) : base("ItemNotContainer", new PyDictionary{["item"] = itemInfo})
        {
        }

        public ItemNotContainer(int itemID) : this(itemID.ToString())
        {
        }
    }
}