using System;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Database;

public abstract class SparseRowsetDatabaseService : MultiClientBoundService
{
    public SparseRowsetHeader RowsetHeader { get; init; }

    protected SparseRowsetDatabaseService (SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Session session, bool keepAlive = false) : base (
        manager, 0, keepAlive
    )
    {
        RowsetHeader = rowsetHeader;
    }

    public abstract PyDataType Fetch (CallInformation call, PyInteger                     startPos,   PyInteger       fetchSize);
    public abstract PyDataType FetchByKey (CallInformation call, PyList                   keyList);
    public abstract PyDataType SelectByUniqueColumnValues (CallInformation call, PyString columnName, PyList          values);

    /// <summary>
    /// Notifies consumers of this SparseRowset that something in the list has changed
    /// </summary>
    /// <param name="primaryKey">The record that has changed</param>
    /// <param name="notificationParams">Extra parameters for the notification if needed</param>
    protected abstract void SendOnObjectChanged (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes, PyDictionary notificationParams = null);

    /// <summary>
    /// Adds a new row to this SparseRowset and notifies bound clients
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="changes"></param>
    public abstract void AddRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes);

    /// <summary>
    /// Notifies bound clients about changes in the data of this SparseRowset
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="changes"></param>
    public abstract void UpdateRow (PyDataType primaryKey, PyDictionary <PyString, PyTuple> changes);

    /// <summary>
    /// Removes a row from this SparseRowset and notifies bound clients
    /// </summary>
    /// <param name="primaryKey"></param>
    public abstract void RemoveRow (PyDataType primaryKey);

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        throw new NotImplementedException ();
    }

    protected override PyDataType MachoBindObject (CallInformation call, ServiceBindParams bindParams, PyDataType callInfo)
    {
        throw new NotImplementedException ();
    }

    protected override MultiClientBoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        throw new NotImplementedException ();
    }

    public PyDataType MachoBindObject (PyDictionary dictPayload, Session session)
    {
        // TODO: the expiration time is 1 day, might be better to properly support this?
        // TODO: investigate these a bit more closely in the future
        // TODO: i'm not so sure about the expiration time
        PyTuple boundServiceInformation = new PyTuple (3)
        {
            [0] = BoundString,
            [1] = dictPayload,
            [2] = DateTime.UtcNow.Add (TimeSpan.FromDays (1)).ToFileTime ()
        };

        return new PySubStruct (new PySubStream (boundServiceInformation));
    }

    /// <summary>
    /// Ensures the client is registered in the list
    /// </summary>
    /// <param name="session">Session to register</param>
    public void BindToSession (Session session)
    {
        Sessions.TryAdd (session.CharacterID, session);
    }
}