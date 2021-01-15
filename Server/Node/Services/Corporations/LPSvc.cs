using Common.Services;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class LPSvc : Service
    {
        private CorporationDB DB { get; }
        
        public LPSvc(CorporationDB db)
        {
            this.DB = db;
        }
        
        public PyDecimal GetLPForCharacterCorp (PyInteger corporationID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.DB.GetLPForCharacterCorp(corporationID, (int) client.CharacterID);
        }
    }
}