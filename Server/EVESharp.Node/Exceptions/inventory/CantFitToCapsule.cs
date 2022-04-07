using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory;

public class CantFitToCapsule : UserError
{
    public CantFitToCapsule () : base ("CantFitToCapsule") { }
}