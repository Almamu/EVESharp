using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class warRegistry : BoundService
    {
        private int mObjectID;
        
        public warRegistry(BoundServiceManager manager) : base(manager, null)
        {
        }

        private warRegistry(BoundServiceManager manager, int objectID, Client client) : base(manager, client)
        {
            this.mObjectID = objectID;
        }

        public override PyInteger MachoResolveObject(PyTuple objectData, PyInteger zero, CallInformation call)
        {
            /*
             * [0] => alliance or corp id
             * [1] => is master
             */
            return this.BoundServiceManager.Container.NodeID;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            return new warRegistry(this.BoundServiceManager, tupleData[0] as PyInteger, call.Client);
        }

        public PyDataType GetWars(PyInteger ownerID, CallInformation call)
        {
            return new PyWarInfo();
        }
    }
}