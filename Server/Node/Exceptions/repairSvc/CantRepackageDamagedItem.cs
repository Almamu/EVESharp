using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.repairSvc
{
    public class CantRepackageDamagedItem : UserError
    {
        public CantRepackageDamagedItem() : base("CantRepackageDamagedItem")
        {
        }
    }
}