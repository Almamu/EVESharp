using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class TheItemIsNotYoursToTake : UserError
    {
        public TheItemIsNotYoursToTake(string itemInfo) : base("TheItemIsNotYoursToTake", new PyDictionary{["item"] = itemInfo})
        {
        }

        public TheItemIsNotYoursToTake(int itemID) : this(itemID.ToString())
        {
        }
    }
}