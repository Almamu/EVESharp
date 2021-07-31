using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class OnlyActiveCEOCanCreateAlliance : UserError
    {
        public OnlyActiveCEOCanCreateAlliance() : base("OnlyActiveCEOCanCreateAlliance")
        {
        }
    }
}