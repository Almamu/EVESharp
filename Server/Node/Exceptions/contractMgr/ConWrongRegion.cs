using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConWrongRegion : UserError
    {
        public ConWrongRegion() : base("ConWrongRegion")
        {
        }
    }
}