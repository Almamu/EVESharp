using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

public class FailedPlugInImplant : UserError
{
    public FailedPlugInImplant () : base ("FailedPlugInImplant") { }
}