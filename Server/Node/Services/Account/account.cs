using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Account
{
    public class account : Service
    {
        private CharacterDB DB { get; }
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        public account(CharacterDB db, ItemManager itemManager, CacheStorage cacheStorage)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.CacheStorage = cacheStorage;
        }

        private PyDataType GetCashBalance(Client client)
        {
            Character character = this.ItemManager.LoadItem(client.EnsureCharacterIsSelected()) as Character;

            return character.Balance;
        }

        public PyDataType GetCashBalance(PyBool isCorpWallet, CallInformation call)
        {
            return this.GetCashBalance(isCorpWallet ? 1 : 0, 1000, call);
        }
        
        public PyDataType GetCashBalance(PyInteger isCorpWallet, CallInformation call)
        {
            return this.GetCashBalance(isCorpWallet, 1000, call);
        }

        public PyDataType GetCashBalance(PyInteger isCorpWallet, PyInteger walletKey, CallInformation call)
        {
            if (isCorpWallet == 0)
                return this.GetCashBalance(call.Client);
            
            throw new CustomError("This function is not supported yet");
        }

        public PyDataType GetKeyMap(CallInformation call)
        {
            return this.DB.GetKeyMap();
        }

        public PyDataType GetEntryTypes(CallInformation call)
        {
            this.CacheStorage.Load(
                "account",
                "GetEntryTypes",
                "SELECT refTypeID AS entryTypeID, refTypeText AS entryTypeName, description FROM market_refTypes",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("account", "GetEntryTypes");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetJournal(PyInteger marketKey, PyInteger fromDate, PyInteger entryTypeID,
            PyBool isCorpWallet, PyInteger transactionID, PyInteger rev, CallInformation call)
        {
            return this.DB.GetJournal(call.Client.EnsureCharacterIsSelected(), entryTypeID, marketKey, fromDate);
        }
    }
}