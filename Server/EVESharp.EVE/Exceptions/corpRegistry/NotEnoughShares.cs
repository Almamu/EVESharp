using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class NotEnoughShares : UserError
{
    public NotEnoughShares (int amount, uint balance) : base (
        "NotEnoughShares", new PyDictionary
        {
            ["amount"]  = FormatAmount (amount),
            ["balance"] = FormatAmount (balance)
        }
    ) { }
}