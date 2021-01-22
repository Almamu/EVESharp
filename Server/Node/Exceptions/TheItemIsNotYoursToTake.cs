using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
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