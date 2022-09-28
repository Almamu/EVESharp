using EVESharp.Database.Old;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.Types;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class  factory : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    public          FactoryDB   DB          { get; }

    public factory (FactoryDB db)
    {
        DB = db;
    }

    public PyDataType GetBlueprintAttributes (ServiceCall call, PyInteger blueprintID)
    {
        return DB.GetBlueprintAttributes (blueprintID, call.Session.CharacterID);
    }

    public PyDataType GetMaterialsForTypeWithActivity (ServiceCall call, PyInteger blueprintTypeID, PyInteger _)
    {
        return DB.GetMaterialsForTypeWithActivity (blueprintTypeID);
    }

    public PyDataType GetMaterialCompositionOfItemType (ServiceCall call, PyInteger typeID)
    {
        return DB.GetMaterialCompositionOfItemType (typeID);
    }

    public PyDataType GetBlueprintInformationAtLocation (ServiceCall call, PyInteger hangarID, PyInteger one)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocation (hangarID);
    }

    public PyDataType GetBlueprintInformationAtLocationWithFlag (ServiceCall call, PyInteger hangarID, PyInteger flag, PyInteger one)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocationWithFlag (hangarID, flag);
    }
}