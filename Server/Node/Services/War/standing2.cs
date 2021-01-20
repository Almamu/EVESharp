using System.Web;
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
            if (call.Client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return new PyTuple(new PyDataType[]
            {
                this.DB.GetCharStandings((int) call.Client.CharacterID),
                this.DB.GetCharPrime((int) call.Client.CharacterID),
                this.DB.GetCharNPCStandings((int) call.Client.CharacterID)
            });
        }

        public PyDataType GetStandingTransactions(PyInteger from, PyInteger to, PyInteger direction, PyInteger eventID,
            PyInteger eventTypeID, PyInteger eventDateTime, CallInformation call)
        {
            if (from != call.Client.CorporationID && from != call.Client.CharacterID && to != call.Client.CorporationID &&
                to != call.Client.CharacterID)
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
            this.DB.CreateStandingTransaction((int) StandingEventType.StandingPlayerSetStanding, (int) call.Client.CharacterID, characterID, standing, reason);
            this.DB.SetPlayerStanding((int) call.Client.CharacterID, characterID, standing);
            
            // send standing change notification to the player
            PyTuple notification = new PyTuple(3)
            {
                [0] = call.Client.CharacterID,
                [1] = characterID,
                [2] = standing
            };
            
            // send the same notification to both players
            call.Client.ClusterConnection.SendNotification("OnStandingSet", "charid", (int) call.Client.CharacterID, call.Client, notification);
            call.Client.ClusterConnection.SendNotification("OnStandingSet", "charid", characterID, notification);
            
            return null;
        }
    }
}