using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

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