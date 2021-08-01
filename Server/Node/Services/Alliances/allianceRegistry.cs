using System;
using EVE;
using EVE.Packets.Exceptions;
using Node.Database;
using Node.Exceptions.allianceRegistry;
using Node.Exceptions.corpRegistry;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Notifications.Client.Alliances;
using Node.Services.Corporations;
using Node.StaticData.Corporation;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Alliances
{
    public class allianceRegistry : MultiClientBoundService
    {
        private AlliancesDB DB { get; init; }
        private BillsDB BillsDB { get; init; }
        private CorporationDB CorporationDB { get; init; }
        private NotificationManager NotificationManager { get; init; }
        private ItemFactory ItemFactory { get; init; }
        private Alliance Alliance { get; init; }

        public allianceRegistry(CorporationDB corporationDB, AlliancesDB db, ItemFactory itemFactory, NotificationManager notificationManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.CorporationDB = corporationDB;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
        }

        private allianceRegistry(Alliance alliance, CorporationDB corporationDB, AlliancesDB db, ItemFactory itemFactory, NotificationManager notificationManager, MultiClientBoundService parent) : base(parent, alliance.ID)
        {
            this.DB = db;
            this.CorporationDB = corporationDB;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
            this.Alliance = alliance;
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            // TODO: CHECK IF ANY NODE HAS THIS ALLIANCE LOADED
            // TODO: IF NOT, LOAD IT HERE AND RETURN OUR ID
            return this.BoundServiceManager.Container.NodeID;
        }

        public override bool IsClientAllowedToCall(CallInformation call)
        {
            return call.Client.AllianceID == this.ObjectID;
        }

        protected override MultiClientBoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            Alliance alliance = this.ItemFactory.LoadItem<Alliance>(bindParams.ObjectID);
            
            return new allianceRegistry(alliance, this.CorporationDB, this.DB, this.ItemFactory, this.NotificationManager, this);
        }

        public PyDataType GetAlliance(CallInformation call)
        {
            return this.DB.GetAlliance(this.ObjectID);
        }

        public PyDataType GetAlliance(PyInteger allianceID, CallInformation call)
        {
            return this.DB.GetAlliance(allianceID);
        }

        public PyDataType UpdateAlliance(PyString description, PyString url, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Client.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_UPDATE_ALLIANCE_NOT_EXECUTOR);
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_UPDATE_ALLIANCE_NOT_DIRECTOR);

            this.Alliance.Description = description;
            this.Alliance.Url = url;
            this.Alliance.Persist();

            OnAllianceChanged change = new OnAllianceChanged(this.ObjectID);

            change
                .AddChange("description", this.Alliance.Description, description)
                .AddChange("url", this.Alliance.Url, url)
                .AddChange("executorCorpID", this.Alliance.ExecutorCorpID, this.Alliance.ExecutorCorpID)
                .AddChange("creatorCorpID", this.Alliance.CreatorCorpID, this.Alliance.CreatorCorpID)
                .AddChange("creatorCharID", this.Alliance.CreatorCharID, this.Alliance.CreatorCharID)
                .AddChange("dictatorial", this.Alliance.Dictatorial, this.Alliance.Dictatorial)
                .AddChange("deleted", false, false);
            
            this.NotificationManager.NotifyAlliance(this.ObjectID, change);

            return null;
        }

        public PyDataType GetRelationships(CallInformation call)
        {
            return this.DB.GetRelationships(this.ObjectID);
        }

        public PyDataType GetAllianceMembers(PyInteger allianceID, CallInformation call)
        {
            return this.DB.GetMembers(allianceID);
        }

        public PyDataType GetMembers(CallInformation call)
        {
            return this.DB.GetMembers(this.ObjectID, true);
        }

        public PyDataType SetRelationship(PyInteger relationship, PyInteger toID, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Client.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_SET_RELATIONSHIP_EXECUTOR_ONLY);
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_SET_RELATIONSHIP_DIRECTOR_ONLY);
            
            this.DB.UpdateRelationship(this.ObjectID, toID, relationship);
                
            OnAllianceRelationshipChanged change =
                new OnAllianceRelationshipChanged(this.ObjectID, toID)
                    .AddChange("toID", null, toID)
                    .AddChange("relationship", null, relationship);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            
            return null;
        }

        public PyDataType DeclareExecutorSupport(PyInteger executorID, CallInformation call)
        {
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DECLARE_EXEC_SUPPORT_DIRECTOR_ONLY);
            
            // get corporation's join date
            long minimumJoinDate = DateTime.UtcNow.AddDays(-7).ToFileTimeUtc();
            long corporationJoinDate = this.CorporationDB.GetAllianceJoinDate(call.Client.CorporationID);

            if (corporationJoinDate > minimumJoinDate)
                throw new CanNotDeclareExecutorInFirstWeek();
            
            // update corporation's supported executor
            this.DB.UpdateSupportedExecutor(call.Client.CorporationID, executorID);

            // calculate the new executor (if any)
            this.DB.CalculateNewExecutorCorp(this.ObjectID, out int? executorCorpID);
            
            OnAllianceMemberChanged change =
                new OnAllianceMemberChanged(this.ObjectID, call.Client.CorporationID)
                    .AddChange("chosenExecutorID", 0, executorID);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            
            // notify the alliance about the executor change
            if (this.Alliance.ExecutorCorpID != executorCorpID)
            {
                OnAllianceChanged allianceChange =
                    new OnAllianceChanged(this.ObjectID)
                        .AddChange("executorCorpID", this.Alliance.ExecutorCorpID, executorCorpID);
                
                this.NotificationManager.NotifyAlliance(this.ObjectID, allianceChange);
            }
            
            // update the executor and store it
            this.Alliance.ExecutorCorpID = executorCorpID;
            this.Alliance.Persist();

            return null;
        }

        public PyDataType GetApplications(CallInformation call)
        {
            return this.DB.GetApplicationsToAlliance(this.ObjectID);
        }

        public PyDataType GetBills(CallInformation call)
        {
            return this.BillsDB.GetBillsPayable(this.ObjectID);
        }

        public PyDataType DeleteRelationship(PyInteger toID, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Client.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY);
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_DIRECTOR_ONLY);
            
            this.DB.RemoveRelationship(this.ObjectID, toID);

            OnAllianceRelationshipChanged change =
                new OnAllianceRelationshipChanged(this.ObjectID, toID)
                    .AddChange("toID", toID, null);

            return null;
        }

        public PyDataType GetRankedAlliances(CallInformation call)
        {
            return this.DB.GetAlliances();
        }
    }
}