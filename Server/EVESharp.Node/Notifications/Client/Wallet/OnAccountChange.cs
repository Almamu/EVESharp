using System.Collections.Generic;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Wallet
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
                WalletKeys.MAIN_WALLET => "cash",
                WalletKeys.SECOND_WALLET => "cash2",
                WalletKeys.THIRD_WALLET => "cash3",
                WalletKeys.FOURTH_WALLET => "cash4",
                WalletKeys.FIFTH_WALLET => "cash5",
                WalletKeys.SIXTH_WALLET => "cash6",
                WalletKeys.SEVENTH_WALLET => "cash7",
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