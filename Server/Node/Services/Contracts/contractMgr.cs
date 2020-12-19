using Common.Database;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Contracts
{
    public class contractMgr : Service
    {
        private ContractDB mDB = null;
        
        public contractMgr(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new ContractDB(db);
        }

        public PyDataType NumRequiringAttention(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            // TODO: PROPERLY IMPLEMENT THIS
            PyDictionary requiringAttention = new PyDictionary();

            requiringAttention["n"] = 0;
            requiringAttention["ncorp"] = 0;
            
            return new PyObjectData("util.KeyVal", requiringAttention);
        }
    }
}