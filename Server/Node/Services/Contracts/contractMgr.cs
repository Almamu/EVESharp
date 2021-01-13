using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Contracts
{
    public class contractMgr : Service
    {
        private ContractDB DB { get; }
        
        public contractMgr(ContractDB db)
        {
            this.DB = db;
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