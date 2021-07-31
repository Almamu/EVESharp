using System;
using System.Collections.Generic;
using Common.Services;
using EVE;
using EVE.Packets.Complex;
using Node.Corporations;
using Node.Database;
using Node.Exceptions.corpRegistry;
using Node.Inventory;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Wallet;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class billMgr : IService
    {
        private CacheStorage CacheStorage { get; }
        private BillsDB DB { get; init; }
        private CorporationDB CorporationDB { get; init; }
        private ItemFactory ItemFactory { get; init; }
        private NotificationManager NotificationManager { get; init; }

        public billMgr(CacheStorage cacheStorage, BillsDB db, CorporationDB corporationDb, MachoNet machoNet, ItemFactory itemFactory, NotificationManager notificationManager)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
            this.CorporationDB = corporationDb;
            this.ItemFactory = itemFactory;
            this.NotificationManager = notificationManager;

            machoNet.OnClusterTimer += this.PerformTimedEvents;
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
            if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED3);
            
            return this.DB.GetBillsReceivable(call.Client.CorporationID);
        }

        public PyDataType GetCorporationBills(CallInformation call)
        {
            // make sure the player has the accountant role
            if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED3);
            
            return this.DB.GetBillsPayable(call.Client.CorporationID);
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