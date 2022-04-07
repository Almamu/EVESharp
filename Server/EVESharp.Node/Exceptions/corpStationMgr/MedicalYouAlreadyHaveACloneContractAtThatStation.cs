using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpStationMgr;

public class MedicalYouAlreadyHaveACloneContractAtThatStation : UserError
{
    public MedicalYouAlreadyHaveACloneContractAtThatStation () : base ("MedicalYouAlreadyHaveACloneContractAtThatStation") { }
}