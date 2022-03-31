using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Network;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.War
{
    public class warRegistry : ClientBoundService
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        private NodeContainer Container { get; init; }
        private int mObjectID;

        public warRegistry(NodeContainer container, BoundServiceManager manager) : base(manager)
        {
            this.Container = container;
        }

        private warRegistry(NodeContainer container, BoundServiceManager manager, int objectID, Session session) : base(manager, session, objectID)
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
            return this.BoundServiceManager.MachoNet.NodeID;
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            return new warRegistry(this.Container, this.BoundServiceManager, bindParams.ObjectID, call.Session);
        }
    }
}