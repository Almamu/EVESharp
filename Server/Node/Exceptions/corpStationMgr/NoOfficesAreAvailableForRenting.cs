using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpStationMgr
{
    public class NoOfficesAreAvailableForRenting : UserError
    {
        public NoOfficesAreAvailableForRenting() : base("NoOfficesAreAvailableForRenting")
        {
        }
    }
}