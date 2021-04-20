using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class CannotRemoveUpgradeManually : UserError
    {
        public CannotRemoveUpgradeManually() : base("CannotRemoveUpgradeManually")
        {
        }
    }
}