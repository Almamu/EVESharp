using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpStationMgr;

public class MedicalThisCloneIsWorse : UserError
{
    public MedicalThisCloneIsWorse() : base("MedicalThisCloneIsWorse")
    {
    }
}