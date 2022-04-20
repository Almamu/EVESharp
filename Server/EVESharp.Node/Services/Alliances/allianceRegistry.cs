using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE.Alliances;
using EVESharp.EVE.Client.Exceptions.allianceRegistry;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.Node.Client.Notifications.Alliances;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Services.Alliances;

public class allianceRegistry : MultiClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    private DatabaseConnection Database       { get; }
    private CorporationDB      CorporationDB  { get; }
    private ChatDB             ChatDB         { get; }
    private NotificationSender Notifications  { get; }
    private ItemFactory        ItemFactory    { get; }
    private Alliance           Alliance       { get; }
    private SessionManager     SessionManager { get; }
    private ClusterManager     ClusterManager { get; }

    public allianceRegistry (
        DatabaseConnection  databaseConnection, CorporationDB  corporationDB, ChatDB chatDB, ItemFactory itemFactory, NotificationSender notificationSender,
        BoundServiceManager manager,            SessionManager sessionManager, ClusterManager clusterManager
    ) : base (manager)
    {
        Database       = databaseConnection;
        CorporationDB  = corporationDB;
        ChatDB         = chatDB;
        Notifications  = notificationSender;
        ItemFactory    = itemFactory;
        SessionManager = sessionManager;
        ClusterManager = clusterManager;

        ClusterManager.OnClusterTimer += PerformTimedEvents;
    }

    private allianceRegistry (
        Alliance alliance, DatabaseConnection databaseConnection, CorporationDB corporationDB, ItemFactory itemFactory, NotificationSender notificationSender,
        SessionManager sessionManager, MultiClientBoundService parent
    ) : base (parent, alliance.ID)
    {
        Database       = databaseConnection;
        CorporationDB  = corporationDB;
        Notifications  = notificationSender;
        ItemFactory    = itemFactory;
        Alliance       = alliance;
        SessionManager = sessionManager;
    }

    private IEnumerable <ApplicationEntry> GetAcceptedAlliances (long minimumTime)
    {
        // TODO: THIS ONE MIGHT HAVE A BETTER PLACE SOMEWHERE, BUT FOR NOW IT'LL LIVE HERE
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT corporationID, allianceID, executorCorpID FROM crpApplications LEFT JOIN crpAlliances USING(allianceID) WHERE `state` = {(int) ApplicationStatus.Accepted} AND applicationUpdateTime < @limit",
            new Dictionary <string, object> {{"@limit", minimumTime}}
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                yield return new ApplicationEntry
                {
                    CorporationID  = reader.GetInt32 (0),
                    AllianceID     = reader.GetInt32 (1),
                    ExecutorCorpID = reader.GetInt32OrNull (2)
                };
        }
    }

    private void PerformTimedEvents (object sender, EventArgs e)
    {
        long minimumTime = DateTime.UtcNow.AddHours (-24).ToFileTimeUtc ();

        // check alliances that were accepted more than 24 hours ago
        foreach (ApplicationEntry entry in this.GetAcceptedAlliances (minimumTime))
        {
            // first update the corporation to join it to the alliance in the database
            CorporationDB.UpdateCorporationInformation (entry.CorporationID, entry.AllianceID, DateTime.UtcNow.ToFileTimeUtc (), entry.ExecutorCorpID);
            // now check if any node has it loaded and notify it about the changes
            long nodeID = ItemFactory.ItemDB.GetItemNode (entry.CorporationID);

            if (nodeID > 0)
            {
                OnCorporationChanged change =
                    new OnCorporationChanged (entry.CorporationID, entry.AllianceID, entry.ExecutorCorpID);

                // notify the node that owns the reference to this corporation
                Notifications.NotifyNode (nodeID, change);
            }

            foreach (int characterID in CorporationDB.GetMembersForCorp (entry.CorporationID))
            {
                // join the player to the corp channel
                ChatDB.JoinEntityChannel (entry.AllianceID, characterID);
                ChatDB.JoinChannel (entry.AllianceID, characterID);
            }

            // send session change based on the corporationID, only one packet needed
            SessionManager.PerformSessionUpdate (Session.CORP_ID, entry.CorporationID, new Session {[Session.ALLIANCE_ID] = entry.AllianceID});
        }

        // finally remove all the applications
        Database.CrpAlliancesHousekeepApplications (minimumTime);
    }


    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        return Database.CluResolveAddress ("allianceRegistry", parameters.ObjectID);
    }

    public override bool IsClientAllowedToCall (Session session)
    {
        return session.AllianceID == ObjectID;
    }

    protected override MultiClientBoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
         if (this.MachoResolveObject (bindParams, call) != BoundServiceManager.MachoNet.NodeID)
             throw new CustomError ("Trying to bind an object that does not belong to us!");

        Alliance alliance = ItemFactory.LoadItem <Alliance> (bindParams.ObjectID);

        return new allianceRegistry (alliance, Database, CorporationDB, ItemFactory, Notifications, SessionManager, this);
    }

    public PyDataType GetAlliance (CallInformation call)
    {
        return this.GetAlliance (ObjectID, call);
    }

    public PyDataType GetAlliance (PyInteger allianceID, CallInformation call)
    {
        return Database.CrpAlliancesGet (allianceID);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_UPDATE_ALLIANCE_NOT_DIRECTOR, CorporationRole.Director)]
    public PyDataType UpdateAlliance (PyString description, PyString url, CallInformation call)
    {
        if (Alliance.ExecutorCorpID != call.Session.CorporationID)
            throw new CrpAccessDenied (MLS.UI_CORP_UPDATE_ALLIANCE_NOT_EXECUTOR);

        Alliance.Description = description;
        Alliance.Url         = url;
        Alliance.Persist ();

        OnAllianceChanged change = new OnAllianceChanged (ObjectID);

        change
            .AddChange ("description",    Alliance.Description,    description)
            .AddChange ("url",            Alliance.Url,            url)
            .AddChange ("executorCorpID", Alliance.ExecutorCorpID, Alliance.ExecutorCorpID)
            .AddChange ("creatorCorpID",  Alliance.CreatorCorpID,  Alliance.CreatorCorpID)
            .AddChange ("creatorCharID",  Alliance.CreatorCharID,  Alliance.CreatorCharID)
            .AddChange ("dictatorial",    Alliance.Dictatorial,    Alliance.Dictatorial)
            .AddChange ("deleted",        false,                   false);

        Notifications.NotifyAlliance (ObjectID, change);

        return null;
    }

    public PyDataType GetRelationships (CallInformation call)
    {
        return Database.CrpAlliancesGetRelationships (this.ObjectID);
    }

    public PyDataType GetAllianceMembers (PyInteger allianceID, CallInformation call)
    {
        return Database.CrpAlliancesGetMembersPublic (allianceID);
    }

    public PyDataType GetMembers (CallInformation call)
    {
        return Database.CrpAlliancesGetMembersPrivate (this.ObjectID);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_SET_RELATIONSHIP_DIRECTOR_ONLY, CorporationRole.Director)]
    public PyDataType SetRelationship (PyInteger relationship, PyInteger toID, CallInformation call)
    {
        if (Alliance.ExecutorCorpID != call.Session.CorporationID)
            throw new CrpAccessDenied (MLS.UI_CORP_SET_RELATIONSHIP_EXECUTOR_ONLY);

        Database.CrpAlliancesUpdateRelationship (this.ObjectID, toID, relationship);

        OnAllianceRelationshipChanged change =
            new OnAllianceRelationshipChanged (ObjectID, toID)
                .AddChange ("toID",         null, toID)
                .AddChange ("relationship", null, relationship);

        Notifications.NotifyAlliance (ObjectID, change);

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_DECLARE_EXEC_SUPPORT_DIRECTOR_ONLY, CorporationRole.Director)]
    public PyDataType DeclareExecutorSupport (PyInteger executorID, CallInformation call)
    {
        // get corporation's join date
        long minimumJoinDate     = DateTime.UtcNow.AddDays (-7).ToFileTimeUtc ();
        long corporationJoinDate = CorporationDB.GetAllianceJoinDate (call.Session.CorporationID);

        if (corporationJoinDate > minimumJoinDate)
            throw new CanNotDeclareExecutorInFirstWeek ();

        // update corporation's supported executor and get the new alliance's executor id back (if any)
        int? executorCorpID = Database.CrpAlliancesUpdateSupportedExecutor (call.Session.CorporationID, executorID, this.ObjectID);

        OnAllianceMemberChanged change =
            new OnAllianceMemberChanged (ObjectID, call.Session.CorporationID)
                .AddChange ("chosenExecutorID", 0, executorID);

        Notifications.NotifyAlliance (ObjectID, change);

        // notify the alliance about the executor change
        if (Alliance.ExecutorCorpID != executorCorpID)
        {
            OnAllianceChanged allianceChange =
                new OnAllianceChanged (ObjectID)
                    .AddChange ("executorCorpID", Alliance.ExecutorCorpID, executorCorpID);

            Notifications.NotifyAlliance (ObjectID, allianceChange);
        }

        // update the executor and store it
        Alliance.ExecutorCorpID = executorCorpID;
        Alliance.Persist ();

        return null;
    }

    public PyDataType GetApplications (CallInformation call)
    {
        return Database.CrpAlliancesListApplications (this.ObjectID);
    }

    public PyDataType GetBills (CallInformation call)
    {
        return Database.CRowset (
            BillsDB.GET_PAYABLE,
            new Dictionary <string, object> {{"_debtorID", ObjectID}}
        );
    }

    [MustHaveCorporationRole(MLS.UI_CORP_DELETE_RELATIONSHIP_DIRECTOR_ONLY, CorporationRole.Director)]
    public PyDataType DeleteRelationship (PyInteger toID, CallInformation call)
    {
        if (Alliance.ExecutorCorpID != call.Session.CorporationID)
            throw new CrpAccessDenied (MLS.UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY);
        
        Database.CrpAlliancesRemoveRelationship (this.ObjectID, toID);

        OnAllianceRelationshipChanged change =
            new OnAllianceRelationshipChanged (ObjectID, toID)
                .AddChange ("toID", toID, null);

        return null;
    }

    public PyDataType GetRankedAlliances (CallInformation call)
    {
        return Database.CrpAlliancesList ();
    }

    [MustHaveCorporationRole(MLS.UI_CORP_DELETE_RELATIONSHIP_DIRECTOR_ONLY, CorporationRole.Director)]
    public PyDataType UpdateApplication (PyInteger corporationID, PyString message, PyInteger newStatus, CallInformation call)
    {
        if (Alliance.ExecutorCorpID != call.Session.CorporationID)
            throw new CrpAccessDenied (MLS.UI_CORP_DELETE_RELATIONSHIP_EXECUTOR_ONLY);

        switch ((int) newStatus)
        {
            case (int) ApplicationStatus.Accepted:
            case (int) ApplicationStatus.Rejected:
                Database.Procedure (
                    "CrpAlliancesUpdateApplication",
                    new Dictionary <string, object>
                    {
                        {"_corporationID", corporationID},
                        {"_allianceID", ObjectID},
                        {"_newStatus", newStatus},
                        {"_currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
                    }
                );

                break;

            default:
                throw new CustomError ("Unknown status for alliance application");
        }

        OnAllianceApplicationChanged change =
            new OnAllianceApplicationChanged (ObjectID, corporationID)
                .AddChange ("allianceID",    ObjectID,                            ObjectID)
                .AddChange ("corporationID", corporationID,                       corporationID)
                .AddChange ("state",         (int) ApplicationStatus.New, newStatus);

        Notifications.NotifyAlliance (ObjectID, change);
        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.Director, change);

        return null;
    }
}