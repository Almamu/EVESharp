using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConCannotTradeNonSingletonShip : UserError
{
    public ConCannotTradeNonSingletonShip (Type ship, int stationID) : base (
        "ConCannotTradeNonSingletonShip", new PyDictionary
        {
            ["example"] = FormatTypeIDAsName (ship.ID),
            ["station"] = FormatLocationID (stationID)
        }
    ) { }
}