using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class FailedPlugInImplant : UserError
{
    public FailedPlugInImplant () : base ("FailedPlugInImplant") { }
}