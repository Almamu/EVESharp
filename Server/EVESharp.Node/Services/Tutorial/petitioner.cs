using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Tutorial;

public class petitioner : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType GetCategoryHierarchicalInfo (CallInformation call)
    {
        throw new CustomError ("Petitions are disabled");
    }

    public PyDataType GetMyPetitionsEx (CallInformation call)
    {
        throw new CustomError ("Petitions are disabled");
    }
}