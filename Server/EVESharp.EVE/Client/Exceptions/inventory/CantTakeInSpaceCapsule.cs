using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CantTakeInSpaceCapsule : UserError
{
    public CantTakeInSpaceCapsule () : base ("CantTakeInSpaceCapsule") { }
}