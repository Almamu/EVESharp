using Node.Inventory.Items.Types;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class account : Service
    {
        public account(ServiceManager manager) : base(manager)
        {
        }

        private PyDataType GetCashBalance(Client client)
        {
            if (client.CharacterID == null)
                return 0;
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return character.Balance;
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
            
            // TODO: get key and search for the correct wallet
            return 0;
        }
    }
}