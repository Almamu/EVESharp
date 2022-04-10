using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Database;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class factory : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    public          FactoryDB   DB          { get; }

    public factory (FactoryDB db)
    {
        DB = db;
    }

    public PyDataType GetBlueprintAttributes (PyInteger blueprintID, CallInformation call)
    {
        return DB.GetBlueprintAttributes (blueprintID, call.Session.CharacterID);
    }

    public PyDataType GetMaterialsForTypeWithActivity (PyInteger blueprintTypeID, PyInteger _, CallInformation call)
    {
        return DB.GetMaterialsForTypeWithActivity (blueprintTypeID);
    }

    public PyDataType GetMaterialCompositionOfItemType (PyInteger typeID, CallInformation call)
    {
        return DB.GetMaterialCompositionOfItemType (typeID);
    }

    public PyDataType GetBlueprintInformationAtLocation (PyInteger hangarID, PyInteger one, CallInformation call)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocation (hangarID);
    }

    public PyDataType GetBlueprintInformationAtLocationWithFlag (PyInteger hangarID, PyInteger flag, PyInteger one, CallInformation call)
    {
        // TODO: IMPLEMENT PROPER PERMISSION CHECKING
        return DB.GetBlueprintInformationAtLocationWithFlag (hangarID, flag);
    }
}