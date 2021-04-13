using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

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