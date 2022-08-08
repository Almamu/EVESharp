using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.repairSvc;

public class CantRepackageDamagedItem : UserError
{
    public CantRepackageDamagedItem () : base ("CantRepackageDamagedItem") { }
}