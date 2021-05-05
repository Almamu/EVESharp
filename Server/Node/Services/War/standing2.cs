using Common.Services;
using EVE.Packets.Complex;
using EVE.Packets.Exceptions;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Notifications.Client.Character;
using Node.StaticData.Standings;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class standing2 : IService
    {
        private StandingDB DB { get; }
        private CacheStorage CacheStorage { get; }
        private ItemFactory ItemFactory { get; }
        private NotificationManager NotificationManager { get; }
        
        public standing2(CacheStorage cacheStorage, StandingDB db, ItemFactory itemFactory, NotificationManager notificationManager)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
            this.ItemFactory = itemFactory;
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

            return CachedMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("standing2", "GetNPCNPCStandings")
            );
        }

        public PyTuple GetCharStandings(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            return new PyTuple(3)
            {
                [0] = this.DB.GetStandings(callerCharacterID),
                [1] = this.DB.GetPrime(callerCharacterID),
                [2] = this.DB.GetNPCStandings(callerCharacterID)
            };
        }

        public PyTuple GetCorpStandings(CallInformation call)
        {
            int corporationID = call.Client.CorporationID;

            return new PyTuple(3)
            {
                [0] = this.DB.GetStandings(corporationID),
                [1] = this.DB.GetPrime(corporationID),
                [2] = this.DB.GetNPCStandings(corporationID)
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
            if (this.ItemFactory.TryGetItem(characterID, out Character character) == true)
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
            return this.DB.GetNPCStandings(characterID);
        }

        public PyDataType SetPlayerStanding(PyInteger characterID, PyDecimal standing, PyString reason, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            this.DB.CreateStandingTransaction((int) EventType.StandingPlayerSetStanding, callerCharacterID, characterID, standing, reason);
            this.DB.SetPlayerStanding(callerCharacterID, characterID, standing);
            
            // send the same notification to both players
            this.NotificationManager.NotifyCharacters(
                new PyList<PyInteger>(2) { [0] = callerCharacterID, [1] = characterID },
                new OnStandingSet(callerCharacterID, characterID, standing)
            );

            return null;
        }
    }
}