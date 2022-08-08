using System.Collections.Generic;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Wallet;

public class OnAccountChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnAccountChange";

    public int    AccountKey { get; init; }
    public string Wallet     { get; init; }
    public int    OwnerID    { get; init; }
    public double NewBalance { get; init; }

    public OnAccountChange (int accountKey, int ownerID, double newBalance) : base (NOTIFICATION_NAME)
    {
        AccountKey = accountKey;
        OwnerID    = ownerID;
        NewBalance = newBalance;

        Wallet = AccountKey switch
        {
            WalletKeys.MAIN    => "cash",
            WalletKeys.SECOND  => "cash2",
            WalletKeys.THIRD   => "cash3",
            WalletKeys.FOURTH  => "cash4",
            WalletKeys.FIFTH   => "cash5",
            WalletKeys.SIXTH   => "cash6",
            WalletKeys.SEVENTH => "cash7",
            _            => ""
        };
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            Wallet,
            OwnerID,
            NewBalance
        };
    }
}