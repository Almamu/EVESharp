using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

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