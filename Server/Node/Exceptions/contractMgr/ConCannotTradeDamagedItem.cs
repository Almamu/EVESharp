using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeDamagedItem : UserError
    {
        public ConCannotTradeDamagedItem(string example) : base("ConCannotTradeDamagedItem", new PyDictionary {["example"] = example})
        {
        }
    }
}