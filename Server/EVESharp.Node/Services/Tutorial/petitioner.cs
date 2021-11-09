using EVESharp.Common.Services;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Tutorial
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