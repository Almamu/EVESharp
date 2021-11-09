using EVESharp.Common.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations
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