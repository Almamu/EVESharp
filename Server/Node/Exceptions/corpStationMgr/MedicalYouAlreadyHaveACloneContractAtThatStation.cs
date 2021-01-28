using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.corpStationMgr
{
    public class MedicalYouAlreadyHaveACloneContractAtThatStation : UserError
    {
        public MedicalYouAlreadyHaveACloneContractAtThatStation() : base("MedicalYouAlreadyHaveACloneContractAtThatStation")
        {
        }
    }
}