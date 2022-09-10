using EVESharp.Database.Old;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
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

    public PyDataType GetBlueprintAttributes (CallInformation call, PyInteger blueprintID)
    {
        return DB.GetBlueprintAttributes (blueprintID, call.Session.CharacterID);
    }

    public PyDataType GetMaterialsForTypeWithActivity (CallInformation call, PyInteger blueprintTypeID, PyInteger _)
    {
        return DB.GetMaterialsForTypeWithActivity (blueprintTypeID);
    }

    public PyDataType GetMaterialCompositionOfItemType (CallInformation call, PyInteger typeID)
    {
        return DB.GetMaterialCompositionOfItemType (typeID);
    }

    public PyDataType GetBlueprintInformationAtLocation (CallInformation call, PyInteger hangarID, PyInteger one)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocation (hangarID);
    }

    public PyDataType GetBlueprintInformationAtLocationWithFlag (CallInformation call, PyInteger hangarID, PyInteger flag, PyInteger one)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocationWithFlag (hangarID, flag);
    }
}