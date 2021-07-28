using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corporationSvc
{
    public class ConfirmCreatingMedal : UserError
    {
        public ConfirmCreatingMedal(int cost) : base("ConfirmCreatingMedal", new PyDictionary {["cost"] = cost})
        {
        }
    }
}