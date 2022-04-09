using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Client.Database;
using EVESharp.Node.Services.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations;

public class MembersSparseRowsetService : SparseRowsetDatabaseService
{
    private         Dictionary <PyDataType, int> RowsIndex = new Dictionary <PyDataType, int> ();
    public override AccessLevel                  AccessLevel         => AccessLevel.None;
    private         Corporation                  Corporation         { get; }
    private         CorporationDB                DB                  { get; }
    private         Notifications.Notifications  Notifications { get; }

    public MembersSparseRowsetService (
        Corporation corporation, CorporationDB db, SparseRowsetHeader rowsetHeader, Notifications.Notifications notifications, BoundServiceManager manager,
        Session     session
    ) : base (rowsetHeader, manager, session, true)
    {
        DB          = db;
        Corporation = corporation;

        // get all the indexes based on the key
        this.RowsIndex      = DB.GetMembers (corporation.ID);
        Notifications = notifications;
    }

    public override PyDataType Fetch (PyInteger startPos, PyInteger fetchSize, CallInformation call)
    {
        return DB.GetMembers (Corporation.ID, startPos, fetchSize, RowsetHeader);
    }

    public override PyDataType FetchByKey (PyList keyList, CallInformation call)
    {
        return DB.GetMembers (keyList.GetEnumerable <PyInteger> (), Corporation.ID, RowsetHeader, this.RowsIndex);
    }

    public override PyDataType SelectByUniqueColumnValues (PyString columnName, PyList values, CallInformation call)
    {
        throw new NotImplementedException ();
    }

    protected override void SendOnObjectChanged (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes, PyDictionary notificationParams = null)
    {
        // TODO: UGLY CASTING THAT SHOULD BE POSSIBLE TO DO DIFFERENTLY
        // TODO: NOT TO MENTION THE LINQ USAGE, MAYBE THERE'S A BETTER WAY OF DOING IT
        PyList <PyDataType> characterIDs = new PyList <PyDataType> (Sessions.Select (x => (PyDataType) x.Value.CharacterID).ToList ());

        Notifications.NotifyCharacters (
            characterIDs.GetEnumerable <PyInteger> (),
            new OnObjectPublicAttributesUpdated (primaryKey, this, changes, notificationParams)
        );
    }

    public override void AddRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes)
    {
        // fetch the new ids list
        this.RowsIndex = DB.GetMembers (Corporation.ID);

        // notify the clients
        this.SendOnObjectChanged (primaryKey, changes);
    }

    public override void UpdateRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes)
    {
        this.SendOnObjectChanged (primaryKey, changes);
    }

    public override void RemoveRow (PyDataType primaryKey)
    {
        // fetch the new ids list
        this.RowsIndex = DB.GetMembers (Corporation.ID);

        PyDictionary <PyString, PyTuple> changes = new PyDictionary <PyString, PyTuple>
        {
            ["characterID"] = new PyTuple (2)
            {
                [0] = primaryKey,
                [1] = null
            },
            ["title"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["startDateTime"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["roles"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["rolesAtHQ"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["rolesAtBase"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["rolesAtOther"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["titleMask"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["grantableRoles"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["grantableRolesAtHQ"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["grantableRolesAtBase"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["grantableRolesAtOther"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["divisionID"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["squadronID"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["baseID"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["blockRoles"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            },
            ["gender"] = new PyTuple (2)
            {
                [0] = 0,
                [1] = null
            }
        };

        // notify the clients
        this.SendOnObjectChanged (primaryKey, changes);
    }

    public override bool IsClientAllowedToCall (Session session)
    {
        return session.CorporationID == Corporation.ID;
    }
}