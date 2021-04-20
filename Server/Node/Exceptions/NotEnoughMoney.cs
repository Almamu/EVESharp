using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class NotEnoughMoney : UserError
    {
        public NotEnoughMoney(double balance, double amount) : base("NotEnoughMoney",
            new PyDictionary {["balance"] = balance, ["amount"] = amount})
        {
        }
    }
}