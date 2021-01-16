using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class charmgr : Service
    {
        private CharacterDB DB { get; }
        private ItemManager ItemManager { get; }
        
        public charmgr(CharacterDB db, ItemManager itemManager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
        }
        
        public PyDataType GetPublicInfo(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetPublicInfo(characterID);
        }

        public PyDataType GetPublicInfo3(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetPublicInfo3(characterID);
        }

        public PyDataType GetTopBounties(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetTopBounties();
        }

        public PyDataType AddToBounty(PyInteger characterID, PyInteger bounty, PyDictionary namedPayload, Client client)
        {
            // create the bounty record and update the information in the database
            this.DB.AddToBounty((int) client.CharacterID, characterID, bounty);

            // update our record if the player is loaded in memory
            if (this.ItemManager.IsItemLoaded(characterID) == true)
            {
                Character character = this.ItemManager.GetItem(characterID) as Character;

                character.Bounty += bounty;
            }
            
            return null;
        }

        public PyDataType GetPrivateInfo(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetPrivateInfo(characterID);
        }
    }
}