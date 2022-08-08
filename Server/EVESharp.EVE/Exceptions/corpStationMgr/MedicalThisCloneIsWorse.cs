using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpStationMgr;

public class MedicalThisCloneIsWorse : UserError
{
    public MedicalThisCloneIsWorse () : base ("MedicalThisCloneIsWorse") { }
}