using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Contracts
{
    public class contractMgr : Service
    {
        private ContractDB DB { get; }
        
        public contractMgr(ContractDB db)
        {
            this.DB = db;
        }

        public PyDataType NumRequiringAttention(CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();
            
            // TODO: PROPERLY IMPLEMENT THIS
            PyDictionary requiringAttention = new PyDictionary();

            requiringAttention["n"] = 0;
            requiringAttention["ncorp"] = 0;
            
            return new PyObjectData("util.KeyVal", requiringAttention);
        }
    }
}