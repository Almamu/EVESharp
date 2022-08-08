using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConReturnItemsMissingNonSingleton : UserError
{
    public ConReturnItemsMissingNonSingleton (Type ship, int stationID) : base (
        "ConReturnItemsMissingNonSingleton", new PyDictionary
        {
            ["example"] = FormatTypeIDAsName (ship.ID),
            ["station"] = FormatLocationID (stationID)
        }
    ) { }
}