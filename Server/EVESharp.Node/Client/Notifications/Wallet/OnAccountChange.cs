using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Wallet;
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
            Keys.MAIN    => "cash",
            Keys.SECOND  => "cash2",
            Keys.THIRD   => "cash3",
            Keys.FOURTH  => "cash4",
            Keys.FIFTH   => "cash5",
            Keys.SIXTH   => "cash6",
            Keys.SEVENTH => "cash7",
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