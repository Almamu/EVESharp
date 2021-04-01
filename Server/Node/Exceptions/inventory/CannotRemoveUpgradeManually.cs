using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CannotRemoveUpgradeManually : UserError
    {
        public CannotRemoveUpgradeManually() : base("CannotRemoveUpgradeManually")
        {
        }
    }
}