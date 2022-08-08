using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CantTakeInSpaceCapsule : UserError
{
    public CantTakeInSpaceCapsule () : base ("CantTakeInSpaceCapsule") { }
}