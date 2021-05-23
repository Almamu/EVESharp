using System;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Database
{
    public abstract class SparseRowsetDatabaseService : MultiClientBoundService
    {
        public SparseRowsetHeader RowsetHeader { get; init; }
        
        public abstract PyDataType Fetch(PyInteger startPos, PyInteger fetchSize, CallInformation call);
        public abstract PyDataType FetchByKey(PyList keyList, CallInformation call);
        public abstract PyDataType SelectByUniqueColumnValues(PyString columnName, PyList values, CallInformation call);
        public abstract void SendOnObjectChanged(int primaryKey);

        protected SparseRowsetDatabaseService(SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Client client) : base(manager, 0)
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

        public PyDataType MachoBindObject(PyDictionary dictPayload, Client client)
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
    }
}