using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class LPSvc : IService
    {
        private CorporationDB DB { get; }
        
        public LPSvc(CorporationDB db)
        {
            this.DB = db;
        }
        
        public PyDecimal GetLPForCharacterCorp (PyInteger corporationID, CallInformation call)
        {
            return this.DB.GetLPForCharacterCorp(corporationID, call.Client.EnsureCharacterIsSelected());
        }
    }
}