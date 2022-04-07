using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class CantTakeInSpaceCapsule : UserError
{
    public CantTakeInSpaceCapsule () : base ("CantTakeInSpaceCapsule") { }
}