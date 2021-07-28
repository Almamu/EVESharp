using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corporationSvc
{
    public class ConfirmGivingMedal : UserError
    {
        public ConfirmGivingMedal(int cost) : base("ConfirmGivingMedal", new PyDictionary {["cost"] = cost})
        {
        }
    }
}