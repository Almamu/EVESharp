using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class FailedPlugInImplant : UserError
{
    public FailedPlugInImplant () : base ("FailedPlugInImplant") { }
}