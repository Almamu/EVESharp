using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Wallet
{
    public class OnAccountChange : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnAccountChange";

        public int AccountKey { get; init; }
        public string Wallet { get; init; }
        public int OwnerID { get; init; }
        public double NewBalance { get; init; }
        
        public OnAccountChange(int accountKey, int ownerID, double newBalance) : base(NOTIFICATION_NAME)
        {
            this.AccountKey = accountKey;
            this.OwnerID = ownerID;
            this.NewBalance = newBalance;

            this.Wallet = this.AccountKey switch
            {
                1000 => "cash",
                1001 => "cash2",
                1002 => "cash3",
                1003 => "cash4",
                1004 => "cash5",
                1005 => "cash6",
                1006 => "cash7",
                _ => ""
            };
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Wallet,
                this.OwnerID,
                this.NewBalance
            };
        }
    }
}