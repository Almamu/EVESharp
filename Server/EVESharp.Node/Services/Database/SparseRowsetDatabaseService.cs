using System;
using EVESharp.EVE;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Database;

public abstract class SparseRowsetDatabaseService : MultiClientBoundService
{
    public SparseRowsetHeader RowsetHeader { get; init; }
        
    public abstract PyDataType Fetch(PyInteger                     startPos,   PyInteger       fetchSize, CallInformation call);
    public abstract PyDataType FetchByKey(PyList                   keyList,    CallInformation call);
    public abstract PyDataType SelectByUniqueColumnValues(PyString columnName, PyList          values, CallInformation call);
        
    /// <summary>
    /// Notifies consumers of this SparseRowset that something in the list has changed
    /// </summary>
    /// <param name="primaryKey">The record that has changed</param>
    /// <param name="notificationParams">Extra parameters for the notification if needed</param>
    protected abstract void SendOnObjectChanged(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes, PyDictionary notificationParams = null);

    /// <summary>
    /// Adds a new row to this SparseRowset and notifies bound clients
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="changes"></param>
    public abstract void AddRow(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes);
    /// <summary>
    /// Notifies bound clients about changes in the data of this SparseRowset
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="changes"></param>
    public abstract void UpdateRow(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes);
    /// <summary>
    /// Removes a row from this SparseRowset and notifies bound clients
    /// </summary>
    /// <param name="primaryKey"></param>
    public abstract void RemoveRow(PyDataType primaryKey);

    protected SparseRowsetDatabaseService(SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Session session, bool keepAlive = false) : base(manager, 0, keepAlive)
    {
        this.RowsetHeader = rowsetHeader;
    }

    protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
    {
        throw new NotImplementedException();
    }

    protected override PyDataType MachoBindObject(ServiceBindParams bindParams, PyDataType callInfo, CallInformation call)
    {
        throw new NotImplementedException();
    }

    protected override MultiClientBoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
    {
        throw new NotImplementedException();
    }

    public PyDataType MachoBindObject(PyDictionary dictPayload, Session session)
    {
        // TODO: the expiration time is 1 day, might be better to properly support this?
        // TODO: investigate these a bit more closely in the future
        // TODO: i'm not so sure about the expiration time
        PyTuple boundServiceInformation = new PyTuple(3)
        {
            [0] = this.BoundString,
            [1] = dictPayload,
            [2] = DateTime.UtcNow.Add(TimeSpan.FromDays(1)).ToFileTime()
        };

        return new PySubStruct(new PySubStream(boundServiceInformation));
    }

    /// <summary>
    /// Ensures the client is registered in the list
    /// </summary>
    /// <param name="session">Session to register</param>
    public void BindToSession(Session session)
    {
        this.Sessions.TryAdd(session.EnsureCharacterIsSelected(), session);
    }
}