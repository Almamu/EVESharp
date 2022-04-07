using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Inventory.Items;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.character;

public class FailedPlugInImplant : UserError
{
    public FailedPlugInImplant() : base("FailedPlugInImplant")
    {
    }
}