using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeContraband : UserError
    {
        public ConCannotTradeContraband(string example) : base("ConCannotTradeContraband", new PyDictionary {["example"] = example})
        {
        }
    }
}