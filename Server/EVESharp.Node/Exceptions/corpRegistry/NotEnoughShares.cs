using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class NotEnoughShares : UserError
{
    public NotEnoughShares (int amount, int balance) : base (
        "NotEnoughShares", new PyDictionary
        {
            ["amount"]  = FormatAmount (amount),
            ["balance"] = FormatAmount (balance)
        }
    ) { }
}