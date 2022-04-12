using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

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