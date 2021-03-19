using System.Runtime.CompilerServices;
using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Contracts
{
    public class contractMgr : Service
    {
        // TODO: THE TYPEID FOR THE BOX IS 24445
        private ContractDB DB { get; }
        
        public contractMgr(ContractDB db)
        {
            this.DB = db;
        }

        public PyDataType NumRequiringAttention(CallInformation call)
        {
            return this.DB.NumRequiringAttention(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType NumOutstandingContracts(CallInformation call)
        {
            return this.DB.NumOutstandingContracts(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType CollectMyPageInfo(PyDataType ignoreList, CallInformation call)
        {
            // TODO: TAKE INTO ACCOUNT THE IGNORE LIST
            
            return this.DB.CollectMyPageInfo(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType GetContractListForOwner(PyInteger ownerID, PyInteger contractStatus, PyInteger contractType, PyInteger action, CallInformation call)
        {
            int? startContractID = null;
            PyDataType pyStartContractID = null;

            call.NamedPayload.TryGetValue("startContractID", out pyStartContractID);

            if (pyStartContractID is PyInteger)
                startContractID = pyStartContractID as PyInteger;
            
            int resultsPerPage = call.NamedPayload["num"] as PyInteger;
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetContractsForOwner(characterID, call.Client.CorporationID),
                    ["bids"] = this.DB.GetContractBidsForOwner(characterID, call.Client.CorporationID),
                    ["items"] = this.DB.GetContractItemsForOwner(characterID, call.Client.CorporationID)
                }
            );
        }
    }
}