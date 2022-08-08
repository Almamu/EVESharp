using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpStationMgr;

public class MedicalYouAlreadyHaveACloneContractAtThatStation : UserError
{
    public MedicalYouAlreadyHaveACloneContractAtThatStation () : base ("MedicalYouAlreadyHaveACloneContractAtThatStation") { }
}