using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions;

public class NotEnoughMoney : UserError
{
    public NotEnoughMoney (double balance, double amount) : base (
        "NotEnoughMoney",
        new PyDictionary
        {
            ["balance"] = FormatISK (balance),
            ["amount"]  = FormatISK (amount)
        }
    ) { }
}