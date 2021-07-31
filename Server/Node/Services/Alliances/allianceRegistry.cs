using EVE;
using EVE.Packets.Exceptions;
using Node.Database;
using Node.Exceptions.corpRegistry;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Notifications.Client.Alliances;
using Node.Services.Corporations;
using Node.StaticData.Corporation;
using PythonTypes.Types.Primitives;

namespace Node.Services.Alliances
{
    public class allianceRegistry : MultiClientBoundService
    {
        private AlliancesDB DB { get; init; }
        private NotificationManager NotificationManager { get; init; }
        
        public allianceRegistry(AlliancesDB db, NotificationManager notificationManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.NotificationManager = notificationManager;
        }

        private allianceRegistry(AlliancesDB db, NotificationManager notificationManager, MultiClientBoundService parent, int objectID) : base(parent, objectID)
        {
            this.DB = db;
            this.NotificationManager = notificationManager;
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

            return new allianceRegistry(this.DB, this.NotificationManager, this, bindParams.ObjectID);
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
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied("");
            
            this.DB.UpdateAlliance(this.ObjectID, description, url);

            OnAllianceChanged change = new OnAllianceChanged(this.ObjectID);

            change
                .AddChange("description", null, description)
                .AddChange("url", null, url);

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
            this.DB.UpdateRelationship(this.ObjectID, toID, relationship);
                
            OnAllianceRelationshipChanged change =
                new OnAllianceRelationshipChanged(this.ObjectID, toID)
                    .AddChange("toID", null, toID)
                    .AddChange("relationship", null, relationship);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            
            return null;
        }

        // UI_CORP_DELETE_MEMBER_EXECUTOR_ONLY
        // UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY
        
        public PyDataType DeclareExecutorSupport(PyInteger executorID, CallInformation call)
        {
            // TODO: FIND A GOOD STRING FOR THE ERROR
            // TODO: ENSURE THE CORPORATION IS MORE THAN A WEEK OLD IN THE ALLIANCE
            // TODO: CanNotDeclareExecutorInFirstWeek
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DECLARE_EXEC_SUPPORT_DIRECTOR_ONLY);
            
            this.DB.UpdateSupportedExecutor(call.Client.CorporationID, executorID);

            //corporationID, allianceID
            OnAllianceMemberChanged change =
                new OnAllianceMemberChanged(this.ObjectID, call.Client.CorporationID)
                    .AddChange("chosenExecutorID", 0, executorID);

            this.NotificationManager.NotifyAlliance(this.ObjectID, change);
            
            return null;
        }
    }
}