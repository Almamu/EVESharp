using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
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
        
        public standing2(CacheStorage cacheStorage, StandingDB db, ItemManager itemManager)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
            this.ItemManager = itemManager;
        }

        public PyDataType GetMyKillRights(CallInformation call)
        {
            PyDictionary killRights = new PyDictionary();
            PyDictionary killedRights = new PyDictionary();

            return new PyTuple(new PyDataType[]
            {
                killRights, killedRights
            });
        }

        public PyDataType GetNPCNPCStandings(CallInformation call)
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

        public PyDataType GetCharStandings(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            return new PyTuple(new PyDataType[]
            {
                this.DB.GetCharStandings(callerCharacterID),
                this.DB.GetCharPrime(callerCharacterID),
                this.DB.GetCharNPCStandings(callerCharacterID)
            });
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

        public PyDataType GetSecurityRating(PyInteger characterID, CallInformation call)
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
            call.Client.ClusterConnection.SendNotification("OnStandingSet", "charid", callerCharacterID, call.Client, notification);
            call.Client.ClusterConnection.SendNotification("OnStandingSet", "charid", characterID, notification);
            
            return null;
        }
    }
}