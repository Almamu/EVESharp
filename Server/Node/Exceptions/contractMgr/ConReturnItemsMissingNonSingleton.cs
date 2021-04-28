using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConReturnItemsMissingNonSingleton : UserError
    {
        public ConReturnItemsMissingNonSingleton(Type ship, int stationID) : base("ConReturnItemsMissingNonSingleton", new PyDictionary {["example"] = FormatTypeIDAsName(ship.ID), ["station"] = FormatLocationID(stationID)})
        {
        }
    }
}