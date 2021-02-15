using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class charmgr : Service
    {
        private CharacterDB DB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        
        public charmgr(CharacterDB db, MarketDB marketDB, ItemManager itemManager)
        {
            this.DB = db;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
        }
        
        public PyDataType GetPublicInfo(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetPublicInfo(characterID);
        }

        public PyDataType GetPublicInfo3(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetPublicInfo3(characterID);
        }

        public PyDataType GetTopBounties(CallInformation call)
        {
            return this.DB.GetTopBounties();
        }

        public PyDataType AddToBounty(PyInteger characterID, PyInteger bounty, CallInformation call)
        {
            // get character's object
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            // ensure there's enough balance
            character.EnsureEnoughBalance(bounty);
            
            // subtract balance to the character
            character.Balance -= bounty;
            
            // create a record in the market transactions
            this.MarketDB.CreateJournalForCharacter(
                MarketReference.Bounty, character.ID, null, characterID,
                -bounty, character.Balance, $"Added to bounty prize", 1000
            );
            
            // create the bounty record and update the information in the database
            this.DB.AddToBounty(call.Client.EnsureCharacterIsSelected(), characterID, bounty);

            // update our record if the player is loaded in memory
            if (this.ItemManager.IsItemLoaded(characterID) == true)
            {
                Character destination = this.ItemManager.GetItem(characterID) as Character;

                destination.Bounty += bounty;
                destination.Persist();
            }
            
            // notify the client about the changes
            call.Client.NotifyBalanceUpdate(character.Balance);

            character.Persist();
            
            return null;
        }

        public PyDataType GetPrivateInfo(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetPrivateInfo(characterID);
        }
    }
}