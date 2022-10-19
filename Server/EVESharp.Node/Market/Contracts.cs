using EVESharp.Database;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Market;
using EVESharp.EVE.Notifications;

namespace EVESharp.Node.Market;

public class Contracts : IContracts
{
    private IDatabase  Database   { get; }
    private ContractDB ContractDB { get; }
    private IItems Items { get; }
    private ITypes Types { get; }
    private IWallets Wallets { get; }
    private INotificationSender Notifications { get; }
    private IDogmaNotifications DogmaNotifications { get; }
    
    public Contracts (ContractDB db, IDatabase database, ITypes types, IItems items, IWallets wallets, INotificationSender notificationSender, IDogmaNotifications dogmaNotifications)
    {
        ContractDB         = db;
        Database           = database;
        Items              = items;
        Types              = types;
        Wallets            = wallets;
        Notifications      = notificationSender;
        DogmaNotifications = dogmaNotifications;

    }
    
    public IContract AcquireContract (int contractID)
    {
        return new Contract (contractID, Database, Types, Items, Wallets, Notifications, DogmaNotifications);
    }

    public IContract CreateContract
    (
        int    characterID, int corporationID,  int? allianceID,   ContractTypes type,  int    availability, int    assigneeID, int    expireTime,
        int    duration,    int startStationID, int? endStationID, double        price, double reward,       double collateral, string title,
        string description, int issuerWalletID
    )
    {
        ulong contractID = ContractDB.CreateContract (
            characterID, corporationID, allianceID, type, availability,
            assigneeID, expireTime, duration, startStationID, endStationID, price,
            reward, collateral, title, description, WalletKeys.MAIN
        );

        return AcquireContract ((int) contractID);
    }
}