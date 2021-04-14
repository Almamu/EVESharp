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
        private ItemManager ItemManager { get; }
        private account account { get; }
        
        public charmgr(CharacterDB db, MarketDB marketDB, ItemManager itemManager, account account)
        {
            this.DB = db;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.account = account;
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
            Character character = this.ItemManager.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            // acquire lock for the market balance
            account.WalletLock walletLock = this.account.AcquireLock(character.ID, 1000);

            try
            {
                // ensure the character has enough balance
                this.account.EnsureEnoughBalance(walletLock, bounty);
                // take the balance from the wallet
                this.account.CreateJournalRecord(
                    walletLock, MarketReference.Bounty, null, characterID, -bounty, "Added to bounty price"
                );
            }
            finally
            {
                // ensure the lock is free'd as soon as we're done with it
                this.account.FreeLock(walletLock);
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