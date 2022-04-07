using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions;

public class NotEnoughMoney : UserError
{
    public NotEnoughMoney(double balance, double amount) : base("NotEnoughMoney",
                                                                new PyDictionary {["balance"] = FormatISK(balance), ["amount"] = FormatISK(amount)})
    {
    }
}