using EVE.Packets.Complex;
using Node.Network;
using Node.StaticData;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class warRegistry : ClientBoundService
    {
        private NodeContainer Container { get; init; }
        private int mObjectID;

        public warRegistry(NodeContainer container, BoundServiceManager manager) : base(manager)
        {
            this.Container = container;
        }

        private warRegistry(NodeContainer container, BoundServiceManager manager, int objectID, Client client) : base(manager, client, objectID)
        {
            this.Container = container;
            this.mObjectID = objectID;
        }
        
        public PyDataType GetWars(PyInteger ownerID, CallInformation call)
        {
            return new WarInfo();
        }

        public PyDataType GetCostOfWarAgainst(PyInteger corporationID, CallInformation call)
        {
            return this.Container.Constants[Constants.warDeclarationCost].Value;
        }
        
        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            // TODO: PROPERLY HANDLE THIS
            return this.BoundServiceManager.Container.NodeID;
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            return new warRegistry(this.Container, this.BoundServiceManager, bindParams.ObjectID, call.Client);
        }
    }
}