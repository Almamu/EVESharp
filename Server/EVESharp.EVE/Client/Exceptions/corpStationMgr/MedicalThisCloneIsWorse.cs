using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class MedicalThisCloneIsWorse : UserError
{
    public MedicalThisCloneIsWorse () : base ("MedicalThisCloneIsWorse") { }
}