using System.Threading;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Nodes.Character
{
    public class OnBalanceUpdate : NodeNotification
    {
        public const string NOTIFICATION_NAME = "OnBalanceUpdate";
        
        public int OwnerID { get; init; }
        public int AccountID { get; init; }
        public double NewBalance { get; init; }
        
        public OnBalanceUpdate(int ownerID, int accountID, double newBalance) : base(NOTIFICATION_NAME)
        {
            this.OwnerID = ownerID;
            this.AccountID = accountID;
            this.NewBalance = newBalance;
        }

        protected override PyDataType GetNotification()
        {
            return new PyTuple(3)
            {
                [0] = this.OwnerID,
                [1] = this.AccountID,
                [2] = this.NewBalance
            };
        }
    }
}