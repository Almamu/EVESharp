using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.repairSvc
{
    public class CantRepackageDamagedItem : UserError
    {
        public CantRepackageDamagedItem() : base("CantRepackageDamagedItem")
        {
        }
    }
}