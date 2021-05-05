using Common.Services;
using EVE.Packets.Exceptions;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Tutorial
{
    public class petitioner : IService
    {
        public PyDataType GetCategoryHierarchicalInfo(CallInformation call)
        {
            throw new CustomError("Petitions are disabled");
        }

        public PyDataType GetMyPetitionsEx(CallInformation call)
        {
            throw new CustomError("Petitions are disabled");
        }
    }
}