using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.corpStationMgr
{
    public class MedicalYouAlreadyHaveACloneContractAtThatStation : UserError
    {
        public MedicalYouAlreadyHaveACloneContractAtThatStation() : base("MedicalYouAlreadyHaveACloneContractAtThatStation")
        {
        }
    }
}