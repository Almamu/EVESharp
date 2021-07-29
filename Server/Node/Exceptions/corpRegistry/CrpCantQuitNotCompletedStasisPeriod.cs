using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpCantQuitNotCompletedStasisPeriod : UserError
    {
        public CrpCantQuitNotCompletedStasisPeriod(int characterID, int hours, int hoursleft) : base("CrpCantQuitNotCompletedStasisPeriod",
            new PyDictionary {["charname"] = FormatOwnerID(characterID), ["hour"] = hours, ["hoursleft"] = hoursleft})
        {
        }
    }
}