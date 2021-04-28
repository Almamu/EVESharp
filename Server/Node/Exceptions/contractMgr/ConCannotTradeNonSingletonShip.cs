using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeNonSingletonShip : UserError
    {
        public ConCannotTradeNonSingletonShip(Type ship, int stationID) : base("ConCannotTradeNonSingletonShip", new PyDictionary {["example"] = FormatTypeIDAsName(ship.ID), ["station"] = FormatLocationID(stationID)})
        {
        }
    }
}