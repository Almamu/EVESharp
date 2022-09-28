using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Services.Tutorial;

public class petitioner : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType GetCategoryHierarchicalInfo (ServiceCall call)
    {
        throw new CustomError ("Petitions are disabled");
    }

    public PyDataType GetMyPetitionsEx (ServiceCall call)
    {
        throw new CustomError ("Petitions are disabled");
    }
}