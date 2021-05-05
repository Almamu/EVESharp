using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class NotEnoughShares : UserError
    {
        public NotEnoughShares(int amount, int balance) : base("NotEnoughShares", new PyDictionary {["amount"] = FormatAmount(amount), ["balance"] = FormatAmount(balance)})
        {
        }
    }
}