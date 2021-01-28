using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class warRegistry : BoundService
    {
        private int mObjectID;
        
        public warRegistry(BoundServiceManager manager) : base(manager)
        {
        }

        private warRegistry(BoundServiceManager manager, int objectID) : base(manager)
        {
            this.mObjectID = objectID;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            return new warRegistry(this.BoundServiceManager, tupleData[0] as PyInteger);
        }

        public PyDataType GetWars(PyInteger ownerID, CallInformation call)
        {
            return new PyWarInfo();
        }
    }
}