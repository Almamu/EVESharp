using System;
using System.Collections.Generic;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Services.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations;

public class OfficesSparseRowsetService : SparseRowsetDatabaseService
{
    private readonly Dictionary <PyDataType, int> RowsIndex = new Dictionary <PyDataType, int> ();
    public override  AccessLevel                  AccessLevel => AccessLevel.None;
    private          Corporation                  Corporation { get; }
    private          CorporationDB                DB          { get; }

    public OfficesSparseRowsetService (
        Corporation corporation, CorporationDB db, SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Session session
    ) : base (rowsetHeader, manager, session, true)
    {
        DB          = db;
        Corporation = corporation;

        // get all the indexes based on the key
        this.RowsIndex = DB.GetOffices (corporation.ID);
    }

    public override PyDataType Fetch (PyInteger startPos, PyInteger fetchSize, CallInformation call)
    {
        return DB.GetOffices (Corporation.ID, startPos, fetchSize, RowsetHeader);
    }

    public override PyDataType FetchByKey (PyList keyList, CallInformation call)
    {
        return DB.GetOffices (keyList.GetEnumerable <PyInteger> (), Corporation.ID, RowsetHeader, this.RowsIndex);
    }

    public override PyDataType SelectByUniqueColumnValues (PyString columnName, PyList values, CallInformation call)
    {
        return DB.GetOffices (columnName, values.GetEnumerable <PyInteger> (), Corporation.ID, RowsetHeader, this.RowsIndex);
    }

    protected override void SendOnObjectChanged (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes, PyDictionary notificationParams = null)
    {
        throw new NotImplementedException ();
    }

    public override void AddRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes)
    {
        throw new NotImplementedException ();
    }

    public override void UpdateRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes)
    {
        throw new NotImplementedException ();
    }

    public override void RemoveRow (PyDataType primaryKey)
    {
        throw new NotImplementedException ();
    }

    public override bool IsClientAllowedToCall (Session session)
    {
        return session.CorporationID == Corporation.ID;
    }
}