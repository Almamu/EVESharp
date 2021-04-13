using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

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