using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.War
{
    public class standing2 : Service
    {
        private StandingDB DB { get; }
        private CacheStorage CacheStorage { get; }
        private ItemManager ItemManager { get; }
        
        public standing2(CacheStorage cacheStorage, StandingDB db, ItemManager itemManager)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
            this.ItemManager = itemManager;
        }

        public PyDataType GetMyKillRights(PyDictionary namedPayload, Client client)
        {
            PyDictionary killRights = new PyDictionary();
            PyDictionary killedRights = new PyDictionary();

            return new PyTuple(new PyDataType[]
            {
                killRights, killedRights
            });
        }

        public PyDataType GetNPCNPCStandings(PyDictionary namedPayload, Client client)
        {
            this.CacheStorage.Load(
                "standing2",
                "GetNPCNPCStandings",
                "SELECT fromID, toID, standing FROM npcStandings",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("standing2", "GetNPCNPCStandings");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCharStandings(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return new PyTuple(new PyDataType[]
            {
                this.DB.GetCharStandings((int) client.CharacterID),
                this.DB.GetCharPrime((int) client.CharacterID),
                this.DB.GetCharNPCStandings((int) client.CharacterID)
            });
        }

        public PyDataType GetStandingTransactions(PyInteger from, PyInteger to, PyInteger direction, PyInteger eventID,
            PyInteger eventTypeID, PyInteger eventDateTime, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetStandingTransactions(from, to, direction, eventID, eventTypeID, eventDateTime);
        }

        public PyDataType GetSecurityRating(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            if (this.ItemManager.IsItemLoaded(characterID) == true)
            {
                Character character = this.ItemManager.GetItem(characterID) as Character;

                return character.SecurityRating;
            }
            else
            {
                return this.DB.GetSecurityRating(characterID);
            }
        }
    }
}