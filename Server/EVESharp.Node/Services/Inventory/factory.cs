using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Inventory
{
    public class factory : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        public FactoryDB DB { get; }
        
        public factory(FactoryDB db)
        {
            this.DB = db;
        }
        
        public PyDataType GetBlueprintAttributes(PyInteger blueprintID, CallInformation call)
        {
            return this.DB.GetBlueprintAttributes(blueprintID, call.Session.EnsureCharacterIsSelected());
        }

        public PyDataType GetMaterialsForTypeWithActivity(PyInteger blueprintTypeID, PyInteger _, CallInformation call)
        {
            return this.DB.GetMaterialsForTypeWithActivity(blueprintTypeID);
        }

        public PyDataType GetMaterialCompositionOfItemType(PyInteger typeID, CallInformation call)
        {
            return this.DB.GetMaterialCompositionOfItemType(typeID);
        }

        public PyDataType GetBlueprintInformationAtLocation(PyInteger hangarID, PyInteger one, CallInformation call)
        {
            // TODO: IMPLEMENT PROPER PERMISSION CHECKING
            return this.DB.GetBlueprintInformationAtLocation(hangarID);
        }

        public PyDataType GetBlueprintInformationAtLocationWithFlag(PyInteger hangarID, PyInteger flag, PyInteger one, CallInformation call)
        {
            // TODO: IMPLEMENT PROPER PERMISSION CHECKING
            return this.DB.GetBlueprintInformationAtLocationWithFlag(hangarID, flag);
        }
    }
}