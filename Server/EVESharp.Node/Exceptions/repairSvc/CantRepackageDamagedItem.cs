using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.repairSvc;

public class CantRepackageDamagedItem : UserError
{
    public CantRepackageDamagedItem() : base("CantRepackageDamagedItem")
    {
    }
}