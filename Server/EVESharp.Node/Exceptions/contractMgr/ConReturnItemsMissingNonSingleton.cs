using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr
{
    public class ConReturnItemsMissingNonSingleton : UserError
    {
        public ConReturnItemsMissingNonSingleton(Type ship, int stationID) : base("ConReturnItemsMissingNonSingleton", new PyDictionary {["example"] = FormatTypeIDAsName(ship.ID), ["station"] = FormatLocationID(stationID)})
        {
        }
    }
}