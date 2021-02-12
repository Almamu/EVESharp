using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class corpmgr : Service
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
    }
}