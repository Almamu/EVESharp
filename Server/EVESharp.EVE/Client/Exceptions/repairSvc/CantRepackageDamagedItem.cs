using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.repairSvc;

public class CantRepackageDamagedItem : UserError
{
    public CantRepackageDamagedItem () : base ("CantRepackageDamagedItem") { }
}