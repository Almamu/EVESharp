using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Alliances;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.allianceRegistry;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Alliances;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Services.Alliances
{
    public class allianceRegistry : MultiClientBoundService
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        
        private DatabaseConnection Database { get; init; }
        private CorporationDB CorporationDB { get; init; }
        private ChatDB ChatDB { get; init; }
        private NotificationManager NotificationManager { get; init; }
        private ItemFactory ItemFactory { get; init; }
        private Alliance Alliance { get; init; }
        private SessionManager SessionManager { get; }

        public allianceRegistry(DatabaseConnection databaseConnection, CorporationDB corporationDB, ChatDB chatDB, ItemFactory itemFactory, NotificationManager notificationManager, BoundServiceManager manager, SessionManager sessionManager) : base(manager)
        {
            this.Database = databaseConnection;
            this.CorporationDB = corporationDB;
            this.ChatDB = chatDB;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
            this.SessionManager = sessionManager;
            
            // TODO: RE-IMPLEMENT ON CLUSTER TIMER
            // machoNet.OnClusterTimer += PerformTimedEvents;
        }
        
        private allianceRegistry(Alliance alliance, DatabaseConnection databaseConnection, CorporationDB corporationDB, ItemFactory itemFactory, NotificationManager notificationManager, SessionManager sessionManager, MultiClientBoundService parent) : base(parent, alliance.ID)
        {
            this.Database = databaseConnection;
            this.CorporationDB = corporationDB;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
            this.Alliance = alliance;
            this.SessionManager = sessionManager;
        }

        private IEnumerable<ApplicationEntry> GetAcceptedAlliances(long minimumTime)
        {
            // TODO: THIS ONE MIGHT HAVE A BETTER PLACE SOMEWHERE, BUT FOR NOW IT'LL LIVE HERE
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(
                ref connection,
                $"SELECT corporationID, allianceID, executorCorpID FROM crpApplications LEFT JOIN crpAlliances USING(allianceID) WHERE `state` = {(int) AllianceApplicationStatus.Accepted} AND applicationUpdateTime < @limit",
                new Dictionary<string, object>()
                {
                    {"@limit", minimumTime}
                }
            );
            
            using (connection)
            using (reader)
            {
                while (reader.Read() == true)
                {
                    yield return new ApplicationEntry
                    {
                        CorporationID = reader.GetInt32(0),
                        AllianceID = reader.GetInt32(1),
                        ExecutorCorpID = reader.GetInt32OrNull(2)
                    };                    
                }
            }
        }

        private void PerformTimedEvents(object? sender, EventArgs e)
        {
            long minimumTime = DateTime.UtcNow.AddHours(-24).ToFileTimeUtc();
            
            // check alliances that were accepted more than 24 hours ago
            foreach (ApplicationEntry entry in this.GetAcceptedAlliances(minimumTime))
            {
                // first update the corporation to join it to the alliance in the database
                this.CorporationDB.UpdateCorporationInformation(entry.CorporationID, entry.AllianceID, DateTime.UtcNow.ToFileTimeUtc(), entry.ExecutorCorpID);
                // now check if any node has it loaded and notify it about the changes
                long nodeID = this.ItemFactory.ItemDB.GetItemNode(entry.CorporationID);

                if (nodeID > 0)
                {
                    OnCorporationChanged change =
                        new OnCorporationChanged(entry.CorporationID, entry.AllianceID, entry.ExecutorCorpID);

                    // notify the node that owns the reference to this corporation
                    this.NotificationManager.NotifyNode(nodeID, change);
                }
                
                foreach (int characterID in this.CorporationDB.GetMembersForCorp(entry.CorporationID))
                {
                    // join the player to the corp channel
                    this.ChatDB.JoinEntityChannel(entry.AllianceID, characterID, ChatDB.CHATROLE_CONVERSATIONALIST);
                    this.ChatDB.JoinChannel(entry.AllianceID, characterID, ChatDB.CHATROLE_CONVERSATIONALIST);
                }
                
                // send session change based on the corporationID, only one packet needed
                this.SessionManager.PerformSessionUpdate(Session.CORP_ID, entry.CorporationID, new Session() {[Session.ALLIANCE_ID] = entry.AllianceID});
            }

            // finally remove all the applications
            Database.Procedure(
                AlliancesDB.HOUSEKEEP_APPLICATIONS,
                new Dictionary<string, object>()
                {
                    {"_limit", minimumTime}
                }
            );
        }


        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            // TODO: CHECK IF ANY NODE HAS THIS ALLIANCE LOADED
            // TODO: IF NOT, LOAD IT HERE AND RETURN OUR ID
            return this.BoundServiceManager.MachoNet.NodeID;
        }

        public override bool IsClientAllowedToCall(Session session)
        {
            return session.AllianceID == this.ObjectID;
        }

        protected override MultiClientBoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.MachoNet.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            Alliance alliance = this.ItemFactory.LoadItem<Alliance>(bindParams.ObjectID);
            
            return new allianceRegistry(alliance, this.Database, this.CorporationDB, this.ItemFactory, this.NotificationManager, this.SessionManager, this);
        }

        public PyDataType GetAlliance(CallInformation call)
        {
            return this.GetAlliance(this.ObjectID, call);
        }

        public PyDataType GetAlliance(PyInteger allianceID, CallInformation call)
        {
            return Database.Row(
                AlliancesDB.GET,
                new Dictionary<string, object>()
                {
                    {"_allianceID", allianceID}
                }
            );
        }

        public PyDataType UpdateAlliance(PyString description, PyString url, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Session.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_UPDATE_ALLIANCE_NOT_EXECUTOR);
            if (CorporationRole.Director.Is(call.Session.CorporationRole) == false)
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
            return Database.IndexRowset(
                0, AlliancesDB.GET_RELATIONSHIPS,
                new Dictionary<string, object>()
                {
                    {"_allianceID", this.ObjectID}
                }
            );
        }

        public PyDataType GetAllianceMembers(PyInteger allianceID, CallInformation call)
        {
            return Database.Rowset(
                AlliancesDB.GET_MEMBERS_PUBLIC,
                new Dictionary<string, object>()
                {
                    {"_allianceID", allianceID}
                }
            );
        }

        public PyDataType GetMembers(CallInformation call)
        {
            return Database.IndexRowset(
                0, AlliancesDB.GET_MEMBERS_PRIVATE,
                new Dictionary<string, object>()
                {
                    {"_allianceID", this.ObjectID}
                }
            );
        }

        public PyDataType SetRelationship(PyInteger relationship, PyInteger toID, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Session.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_SET_RELATIONSHIP_EXECUTOR_ONLY);
            if (CorporationRole.Director.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_SET_RELATIONSHIP_DIRECTOR_ONLY);
            
            Database.Procedure(
                AlliancesDB.UPDATE_RELATIONSHIP,
                new Dictionary<string, object>()
                {
                    {"_fromID", this.ObjectID},
                    {"_toID", toID},
                    {"_relationship", relationship}
                }
            );
                
            OnAllianceRelationshipChanged change =
                new OnAllianceRelationshipChanged(this.ObjectID, toID)
                    .AddChange("toID", null, toID)
                    .AddChange("relationship", null, relationship);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            
            return null;
        }

        public PyDataType DeclareExecutorSupport(PyInteger executorID, CallInformation call)
        {
            if (CorporationRole.Director.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DECLARE_EXEC_SUPPORT_DIRECTOR_ONLY);
            
            // get corporation's join date
            long minimumJoinDate = DateTime.UtcNow.AddDays(-7).ToFileTimeUtc();
            long corporationJoinDate = this.CorporationDB.GetAllianceJoinDate(call.Session.CorporationID);

            if (corporationJoinDate > minimumJoinDate)
                throw new CanNotDeclareExecutorInFirstWeek();

            // update corporation's supported executor and get the new alliance's executor id back (if any)
            int? executorCorpID = Database.Scalar<int?>(
                AlliancesDB.UPDATE_SUPPORTED_EXECUTOR,
                new Dictionary<string, object>()
                {
                    {"_corporationID", call.Session.CorporationID},
                    {"_chosenExecutorID", executorID},
                    {"_allianceID", this.ObjectID}
                }
            );
            
            OnAllianceMemberChanged change =
                new OnAllianceMemberChanged(this.ObjectID, call.Session.CorporationID)
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
            return Database.IndexRowset(
                1, AlliancesDB.LIST_APPLICATIONS,
                new Dictionary<string, object>()
                {
                    {"_allianceID", this.ObjectID}
                }
            );
        }

        public PyDataType GetBills(CallInformation call)
        {
            return Database.CRowset(
                BillsDB.GET_PAYABLE,
                new Dictionary<string, object>()
                {
                    {"_debtorID", this.ObjectID}
                }
            );
        }

        public PyDataType DeleteRelationship(PyInteger toID, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Session.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY);
            if (CorporationRole.Director.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_DIRECTOR_ONLY);
            
            Database.Procedure(
                AlliancesDB.REMOVE_RELATIONSHIP,
                new Dictionary<string, object>()
                {
                    {"_fromID", this.ObjectID},
                    {"_toID", toID}
                }
            );

            OnAllianceRelationshipChanged change =
                new OnAllianceRelationshipChanged(this.ObjectID, toID)
                    .AddChange("toID", toID, null);

            return null;
        }

        public PyDataType GetRankedAlliances(CallInformation call)
        {
            return Database.Rowset(
                AlliancesDB.LIST
            );
        }

        public PyDataType UpdateApplication(PyInteger corporationID, PyString message, PyInteger newStatus, CallInformation call)
        {
            if (this.Alliance.ExecutorCorpID != call.Session.CorporationID)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY);
            if (CorporationRole.Director.Is(call.Session.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DELETE_RELATIONSHIP_DIRECTOR_ONLY);

            switch ((int) newStatus)
            {
                case (int) AllianceApplicationStatus.Accepted:
                case (int) AllianceApplicationStatus.Rejected:
                    Database.Procedure(
                        "CrpAlliancesUpdateApplication",
                        new Dictionary<string, object>()
                        {
                            {"_corporationID", corporationID},
                            {"_allianceID", this.ObjectID},
                            {"_newStatus", newStatus},
                            {"_currentTime", DateTime.UtcNow.ToFileTimeUtc()}
                        }
                    );
                    break;
                
                default:
                    throw new CustomError("Unknown status for alliance application");
            }
            
            OnAllianceApplicationChanged change =
                new OnAllianceApplicationChanged(this.ObjectID, corporationID)
                    .AddChange("allianceID", this.ObjectID, this.ObjectID)
                    .AddChange("corporationID", corporationID, corporationID)
                    .AddChange("state", (int) AllianceApplicationStatus.New, newStatus);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.Director, change);

            return null;
        }
    }
}