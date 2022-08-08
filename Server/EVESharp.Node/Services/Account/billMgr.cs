using System;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Cache;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Notifications;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account;

public class billMgr : Service
{
    public override AccessLevel         AccessLevel   => AccessLevel.None;
    private         CacheStorage        CacheStorage  { get; }
    private         CorporationDB       CorporationDB { get; }
    private         ItemFactory         ItemFactory   { get; }
    private         NotificationSender  Notifications { get; }
    private         IDatabaseConnection Database      { get; }

    public billMgr (
        CacheStorage       cacheStorage,       IDatabaseConnection databaseConnection, CorporationDB corporationDb, ItemFactory itemFactory,
        NotificationSender notificationSender, ClusterManager      clusterManager
    )
    {
        CacheStorage  = cacheStorage;
        Database      = databaseConnection;
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
        return Database.MktBillsGetReceivable (call.Session.CorporationID);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_ACCESSDENIED3, CorporationRole.Accountant, CorporationRole.JuniorAccountant)]
    public PyDataType GetCorporationBills (CallInformation call)
    {
        return Database.MktBillsGetPayable (call.Session.CorporationID);
    }

    public PyDataType PayCorporationBill (CallInformation call, PyInteger billID)
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
            int billID = (int) Database.MktBillsCreate (
                BillTypes.RentalBill, office.CorporationID, ownerID,
                office.PeriodCost, dueDate, 0, (int) Types.OfficeFolder, office.StationID
            );
            CorporationDB.SetNextBillID (office.CorporationID, office.OfficeID, billID);

            // notify characters about the new bill
            Notifications.NotifyCorporation (office.CorporationID, new OnBillReceived ());
        }
    }
}