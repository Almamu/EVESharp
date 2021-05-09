using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class corpmgr : IService
    {
        private CorporationDB DB { get; }
        
        public corpmgr(CorporationDB db)
        {
            this.DB = db;
        }
        
        public PyDataType GetPublicInfo(PyInteger corporationID, CallInformation call)
        {
            return this.DB.GetPublicInfo(corporationID);
        }

        public PyDataType GetCorporationIDForCharacter(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetCorporationIDForCharacter(characterID);
        }

        public PyDataType GetAssetInventory(PyInteger corporationID, PyString which, CallInformation call)
        {
            return new PyList();
        }
    }
}