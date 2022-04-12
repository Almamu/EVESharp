using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CantFitToCapsule : UserError
{
    public CantFitToCapsule () : base ("CantFitToCapsule") { }
}