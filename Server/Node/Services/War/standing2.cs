using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class standing2 : Service
    {
        private StandingDB DB { get; }
        private CacheStorage CacheStorage { get; }
        private ItemManager ItemManager { get; }
        private NotificationManager NotificationManager { get; }
        
        public standing2(CacheStorage cacheStorage, StandingDB db, ItemManager itemManager, NotificationManager notificationManager)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
            this.ItemManager = itemManager;
            this.NotificationManager = notificationManager;
        }

        public PyTuple GetMyKillRights(CallInformation call)
        {
            PyDictionary killRights = new PyDictionary();
            PyDictionary killedRights = new PyDictionary();

            return new PyTuple(2)
            {
                [0] = killRights,
                [1] = killedRights
            };
        }

        public PyDataType GetNPCNPCStandings(CallInformation call)
        {
            this.CacheStorage.Load(
                "standing2",
                "GetNPCNPCStandings",
                "SELECT fromID, toID, standing FROM npcStandings",
                CacheStorage.CacheObjectType.Rowset
            );

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("standing2", "GetNPCNPCStandings")
            );
        }

        public PyTuple GetCharStandings(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            return new PyTuple(3)
            {
                [0] = this.DB.GetCharStandings(callerCharacterID),
                [1] = this.DB.GetCharPrime(callerCharacterID),
                [2] = this.DB.GetCharNPCStandings(callerCharacterID)
            };
        }

        public PyDataType GetStandingTransactions(PyInteger from, PyInteger to, PyInteger direction, PyInteger eventID,
            PyInteger eventTypeID, PyInteger eventDateTime, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (from != call.Client.CorporationID && from != callerCharacterID && to != call.Client.CorporationID &&
                to != callerCharacterID)
                throw new CustomError("You can only view standings that concern you");
            
            return this.DB.GetStandingTransactions(from, to, direction, eventID, eventTypeID, eventDateTime);
        }

        public PyDecimal GetSecurityRating(PyInteger characterID, CallInformation call)
        {
            if (this.ItemManager.TryGetItem(characterID, out Character character) == true)
            {
                return character.SecurityRating;
            }
            else
            {
                return this.DB.GetSecurityRating(characterID);
            }
        }

        public PyDataType GetNPCStandingsTo(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetCharNPCStandings(characterID);
        }

        public PyDataType SetPlayerStanding(PyInteger characterID, PyDecimal standing, PyString reason, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            this.DB.CreateStandingTransaction((int) StandingEventType.StandingPlayerSetStanding, callerCharacterID, characterID, standing, reason);
            this.DB.SetPlayerStanding(callerCharacterID, characterID, standing);
            
            // send standing change notification to the player
            PyTuple notification = new PyTuple(3)
            {
                [0] = callerCharacterID,
                [1] = characterID,
                [2] = standing
            };
            
            // send the same notification to both players
            this.NotificationManager.NotifyCharacters(
                new PyDataType[] { callerCharacterID, characterID },
                "OnStandingSet",
                notification
            );

            return null;
        }
    }
}