using Common.Services;
using Node.Database;
using Node.Inventory.Items;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class factory : IService
    {
        public FactoryDB DB { get; }
        
        public factory(FactoryDB db)
        {
            this.DB = db;
        }
        
        public PyDataType GetBlueprintAttributes(PyInteger blueprintID, CallInformation call)
        {
            return this.DB.GetBlueprintAttributes(blueprintID, call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType GetMaterialsForTypeWithActivity(PyInteger blueprintTypeID, PyInteger _, CallInformation call)
        {
            return this.DB.GetMaterialsForTypeWithActivity(blueprintTypeID);
        }

        public PyDataType GetMaterialCompositionOfItemType(PyInteger typeID, CallInformation call)
        {
            return this.DB.GetMaterialCompositionOfItemType(typeID);
        }
    }
}