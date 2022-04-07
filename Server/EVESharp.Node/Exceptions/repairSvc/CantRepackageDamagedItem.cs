using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.repairSvc;

public class CantRepackageDamagedItem : UserError
{
    public CantRepackageDamagedItem () : base ("CantRepackageDamagedItem") { }
}