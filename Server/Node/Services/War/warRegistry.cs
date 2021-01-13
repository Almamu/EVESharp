using System.Runtime.CompilerServices;
using Common.Logging;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.War
{
    public class warRegistry : BoundService
    {
        private int mObjectID;
        
        public warRegistry(BoundServiceManager manager, Logger logger) : base(manager, logger)
        {
        }

        private warRegistry(BoundServiceManager manager, int objectID, Logger logger) : base(manager, logger)
        {
            this.mObjectID = objectID;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            return new warRegistry(this.BoundServiceManager, tupleData[0] as PyInteger, this.Log.Logger);
        }

        public PyDataType GetWars(PyInteger ownerID, PyDictionary namedPayload, Client client)
        {
            return new PyWarInfo();
        }
    }
}