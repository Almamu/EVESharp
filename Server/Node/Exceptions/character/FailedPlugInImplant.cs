using EVE.Packets.Exceptions;
using Node.Inventory.Items;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.character
{
    public class FailedPlugInImplant : UserError
    {
        public FailedPlugInImplant() : base("FailedPlugInImplant")
        {
        }
    }
}