using System;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Services.Account;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class charmgr : IService
    {
        private CharacterDB DB { get; }
        private MarketDB MarketDB { get; }
        private ItemFactory ItemFactory { get; }
        private WalletManager WalletManager { get; }
        
        public charmgr(CharacterDB db, MarketDB marketDB, ItemFactory itemFactory, WalletManager WalletManager)
        {
            this.DB = db;
            this.MarketDB = marketDB;
            this.ItemFactory = itemFactory;
            this.WalletManager = WalletManager;
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
            Character character = this.ItemFactory.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            // access the wallet and do the required changes
            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            {
                // ensure the character has enough balance
                wallet.EnsureEnoughBalance(bounty);
                // take the balance from the wallet
                wallet.CreateJournalRecord(
                    MarketReference.Bounty, null, characterID, -bounty, "Added to bounty price"
                );
            }
            
            // create the bounty record and update the information in the database
            this.DB.AddToBounty(call.Client.EnsureCharacterIsSelected(), characterID, bounty);
            
            return null;
        }

        public PyDataType GetPrivateInfo(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetPrivateInfo(characterID);
        }
    }
}