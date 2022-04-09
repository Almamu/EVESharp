using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class MedicalYouAlreadyHaveACloneContractAtThatStation : UserError
{
    public MedicalYouAlreadyHaveACloneContractAtThatStation () : base ("MedicalYouAlreadyHaveACloneContractAtThatStation") { }
}