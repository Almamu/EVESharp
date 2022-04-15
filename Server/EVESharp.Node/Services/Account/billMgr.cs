using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Corporations;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Cache;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Notifications;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account;

public class billMgr : Service
{
    public override AccessLevel        AccessLevel   => AccessLevel.None;
    private         CacheStorage       CacheStorage  { get; }
    private         BillsDB            DB            { get; }
    private         CorporationDB      CorporationDB { get; }
    private         ItemFactory        ItemFactory   { get; }
    private         NotificationSender Notifications { get; }
    private         DatabaseConnection Database      { get; }

    public billMgr (
        CacheStorage       cacheStorage, DatabaseConnection databaseConnection, BillsDB db, CorporationDB corporationDb, ItemFactory itemFactory,
        NotificationSender notificationSender, ClusterManager clusterManager
    )
    {
        CacheStorage  = cacheStorage;
        Database      = databaseConnection;
        DB            = db;
        CorporationDB = corporationDb;
        ItemFactory   = itemFactory;
        Notifications = notificationSender;

        clusterManager.OnClusterTimer += this.PerformTimedEvents;
    }

    public PyDataType GetBillTypes (CallInformation call)
    {
        CacheStorage.Load (
            "billMgr",
            "GetBillTypes",
            "SELECT billTypeID, billTypeName, description FROM billTypes",
            CacheStorage.CacheObjectType.Rowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("billMgr", "GetBillTypes");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_ACCESSDENIED3, CorporationRole.Accountant, CorporationRole.JuniorAccountant)]
    public PyDataType GetCorporationBillsReceivable (CallInformation call)
    {
        return Database.CRowset (
            BillsDB.GET_RECEIVABLE,
            new Dictionary <string, object> {{"_creditorID", call.Session.CorporationID}}
        );
    }

    [MustHaveCorporationRole(MLS.UI_CORP_ACCESSDENIED3, CorporationRole.Accountant, CorporationRole.JuniorAccountant)]
    public PyDataType GetCorporationBills (CallInformation call)
    {
        return Database.CRowset (
            BillsDB.GET_PAYABLE,
            new Dictionary <string, object> {{"_debtorID", call.Session.CorporationID}}
        );
    }

    public PyDataType PayCorporationBill (PyInteger billID, CallInformation call)
    {
        return null;
    }

    public void PerformTimedEvents (object sender, EventArgs args)
    {
        List <CorporationOffice> offices = CorporationDB.FindOfficesCloseToRenewal ();

        foreach (CorporationOffice office in offices)
        {
            long dueDate = office.DueDate;
            int  ownerID = ItemFactory.Stations [office.StationID].OwnerID;
            int billID = (int) DB.CreateBill (
                BillTypes.RentalBill, office.CorporationID, ownerID,
                office.PeriodCost, dueDate, 0, (int) Types.OfficeFolder, office.StationID
            );
            CorporationDB.SetNextBillID (office.CorporationID, office.OfficeID, billID);

            // notify characters about the new bill
            Notifications.NotifyCorporation (office.CorporationID, new OnBillReceived ());
        }
    }
}