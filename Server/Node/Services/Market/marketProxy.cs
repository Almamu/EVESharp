using System.Threading;
using Common.Database;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Market
{
    public class marketProxy : Service
    {
        private MarketDB mDB = null;
        
        public marketProxy(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new MarketDB(db);
        }

        public PyDataType CharGetNewTransactions(PyInteger sellBuy, PyInteger typeID, PyNone clientID,
            PyInteger quantity, PyNone fromDate, PyNone maxPrice, PyInteger minPrice, PyDictionary namedPayload,
            Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            TransactionType transactionType = TransactionType.Either;

            if (sellBuy is PyInteger)
            {
                switch ((int) (sellBuy as PyInteger))
                {
                    case 0:
                        transactionType = TransactionType.Sell;
                        break;
                    case 1:
                        transactionType = TransactionType.Buy;
                        break;
                }
            }
            
            return this.mDB.CharGetNewTransactions(
                (int) client.CharacterID, clientID, transactionType, typeID as PyInteger, quantity, minPrice
            );
        }

        public PyDataType GetCharOrders(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.mDB.GetCharOrders((int) client.CharacterID);
        }
    }
}