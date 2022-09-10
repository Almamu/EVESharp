using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.inventory;

public class NotEnoughCargoSpace : UserError
{
    public NotEnoughCargoSpace (double volume, double available) : base (
        "NotEnoughCargoSpace", new PyDictionary
        {
            ["volume"]    = FormatAmount (volume),
            ["available"] = FormatAmount (available)
        }
    ) { }
}