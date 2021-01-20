using System.Threading;
using Common.Database;
using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Market
{
    public class marketProxy : Service
    {
        private MarketDB DB { get; }
        
        public marketProxy(MarketDB db)
        {
            this.DB = db;
        }

        public PyDataType CharGetNewTransactions(PyInteger sellBuy, PyInteger typeID, PyNone clientID,
            PyInteger quantity, PyNone fromDate, PyNone maxPrice, PyInteger minPrice, CallInformation call)
        {
            if (call.Client.CharacterID == null)
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
            
            return this.DB.CharGetNewTransactions(
                (int) call.Client.CharacterID, clientID, transactionType, typeID as PyInteger, quantity, minPrice
            );
        }

        public PyDataType GetCharOrders(CallInformation call)
        {
            if (call.Client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.DB.GetCharOrders((int) call.Client.CharacterID);
        }
    }
}