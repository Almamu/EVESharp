using EVE.Packets.Complex;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class warRegistry : ClientBoundService
    {
        private int mObjectID;
        
        public warRegistry(BoundServiceManager manager) : base(manager)
        {
        }

        private warRegistry(BoundServiceManager manager, int objectID, Client client) : base(manager, client, objectID)
        {
            this.mObjectID = objectID;
        }
        
        public PyDataType GetWars(PyInteger ownerID, CallInformation call)
        {
            return new WarInfo();
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            // TODO: PROPERLY HANDLE THIS
            return this.BoundServiceManager.Container.NodeID;
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            return new warRegistry(this.BoundServiceManager, bindParams.ObjectID, call.Client);
        }
    }
}