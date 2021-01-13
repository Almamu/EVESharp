using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
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
            if (client.CharacterID == null)
                return 0;
            
            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return character.Balance;
        }

        public PyDataType GetCashBalance(PyBool isCorpWallet, PyDictionary namedPayload, Client client)
        {
            return this.GetCashBalance(isCorpWallet ? 1 : 0, 1000, namedPayload, client);
        }
        
        public PyDataType GetCashBalance(PyInteger isCorpWallet, PyDictionary namedPayload, Client client)
        {
            return this.GetCashBalance(isCorpWallet, 1000, namedPayload, client);
        }

        public PyDataType GetCashBalance(PyInteger isCorpWallet, PyInteger walletKey, PyDictionary namedPayload,
            Client client)
        {
            if (isCorpWallet == 0)
                return this.GetCashBalance(client);
            
            throw new CustomError("This function is not supported yet");
        }

        public PyDataType GetKeyMap(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetKeyMap();
        }

        public PyDataType GetEntryTypes(PyDictionary namedPayload, Client client)
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
            PyBool isCorpWallet, PyInteger transactionID, PyInteger rev, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetJournal((int) client.CharacterID, entryTypeID, marketKey, fromDate);
        }
    }
}