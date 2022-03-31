using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.Node.Corporations;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Wallet;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account
{
    public class billMgr : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        private CacheStorage CacheStorage { get; }
        private BillsDB DB { get; init; }
        private CorporationDB CorporationDB { get; init; }
        private ItemFactory ItemFactory { get; init; }
        private NotificationManager NotificationManager { get; init; }
        private DatabaseConnection Database { get; init; }

        public billMgr(CacheStorage cacheStorage, DatabaseConnection databaseConnection, BillsDB db, CorporationDB corporationDb, ItemFactory itemFactory, NotificationManager notificationManager)
        {
            this.CacheStorage = cacheStorage;
            this.Database = databaseConnection;
            this.DB = db;
            this.CorporationDB = corporationDb;
            this.ItemFactory = itemFactory;
            this.NotificationManager = notificationManager;

            // TODO: RE-IMPLEMENT ON CLUSTER TIMER
            // machoNet.OnClusterTimer += this.PerformTimedEvents;
        }

        public PyDataType GetBillTypes(CallInformation call)
        {
            this.CacheStorage.Load(
                "billMgr",
                "GetBillTypes",
                "SELECT billTypeID, billTypeName, description FROM billTypes",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("billMgr", "GetBillTypes");

            return CachedMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCorporationBillsReceivable(CallInformation call)
        {
            // make sure the player has the accountant role
            if (CorporationRole.Accountant.Is(call.Session.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED3);
            
            return Database.CRowset(
                BillsDB.GET_RECEIVABLE,
                new Dictionary<string, object>()
                {
                    {"_creditorID", call.Session.CorporationID}
                }
            );
        }

        public PyDataType GetCorporationBills(CallInformation call)
        {
            // make sure the player has the accountant role
            if (CorporationRole.Accountant.Is(call.Session.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED3);
            
            return Database.CRowset(
                BillsDB.GET_PAYABLE,
                new Dictionary<string, object>()
                {
                    {"_debtorID", call.Session.CorporationID}
                }
            );
        }

        public PyDataType PayCorporationBill(PyInteger billID, CallInformation call)
        {
            return null;
        }

        public void PerformTimedEvents(object? sender, EventArgs args)
        {
            List<CorporationOffice> offices = this.CorporationDB.FindOfficesCloseToRenewal();

            foreach (CorporationOffice office in offices)
            {
                long dueDate = office.DueDate;
                int ownerID = this.ItemFactory.Stations[office.StationID].OwnerID;
                int billID = (int) this.DB.CreateBill(
                    BillTypes.RentalBill, office.CorporationID, ownerID,
                    office.PeriodCost, dueDate, 0, (int) Types.OfficeFolder, office.StationID
                );
                this.CorporationDB.SetNextBillID(office.CorporationID, office.OfficeID, billID);
                
                // notify characters about the new bill
                this.NotificationManager.NotifyCorporation(office.CorporationID, new OnBillReceived());
            }
        }
    }
}