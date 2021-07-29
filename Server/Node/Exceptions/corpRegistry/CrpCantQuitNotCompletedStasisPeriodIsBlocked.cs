using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpCantQuitNotCompletedStasisPeriodIsBlocked : UserError
    {
        public CrpCantQuitNotCompletedStasisPeriodIsBlocked(int characterID, int hours, int hoursleft) : base("CrpCantQuitNotCompletedStasisPeriodIsBlocked",
            new PyDictionary {["charname"] = FormatOwnerID(characterID), ["hour"] = hours, ["hoursleft"] = hoursleft})
        {
        }
    }
}