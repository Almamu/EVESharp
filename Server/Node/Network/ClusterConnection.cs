using System;
using Common;
using Common.Network;
using Common.Packets;
using Common.Services;
using Marshal;

namespace Node.Network
{
    public class ClusterConnection
    {
        public EVEClientSocket Socket { get; private set; }
        public NodeContainer Container { get; private set; }
        
        public ClusterConnection(NodeContainer container)
        {
            this.Container = container;
            this.Socket = new EVEClientSocket();
            this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
            this.Socket.SetExceptionHandler(HandleException);
        }

        private void HandleException(Exception ex)
        {
            Log.Error("ClusterConnection", "Exception detected: ");

            do
            {
                Log.Error("ClusterConnection", ex.Message);
                Log.Trace("ClusterConnection", ex.StackTrace);
            } while ((ex = ex.InnerException) != null);
        }
        
        private void ReceiveLowLevelVersionExchangeCallback(PyObject ar)
        {
            try
            {
                LowLevelVersionExchange exchange = this.CheckLowLevelVersionExchange(ar);

                // reply with the node LowLevelVersionExchange
                LowLevelVersionExchange reply = new LowLevelVersionExchange();

                reply.codename = Common.Constants.Game.codename;
                reply.birthday = Common.Constants.Game.birthday;
                reply.build = Common.Constants.Game.build;
                reply.machoVersion = Common.Constants.Game.machoVersion;
                reply.version = Common.Constants.Game.version;
                reply.region = Common.Constants.Game.region;
                reply.isNode = true;

                this.Socket.Send(reply);
                
                // set the new handler
                this.Socket.SetReceiveCallback(ReceiveNodeInfoCallback);
            }
            catch (Exception e)
            {
                Log.Error("LowLevelVersionExchange", e.Message);
                throw;
            }
        }

        private void ReceiveNodeInfoCallback(PyObject ar)
        {
            if (ar is PyObjectData == false)
                throw new Exception($"Expected PyObjectData for machoNet.nodeInfo but got {ar.Type}");
        
            PyObjectData info = ar as PyObjectData;

            if(info.Name != "machoNet.nodeInfo")
                throw new Exception($"Expected PyObjectData of type machoNet.nodeInfo but got {info.Name}");
        
            // Update our local info
            NodeInfo nodeinfo = new NodeInfo();

            nodeinfo.Decode(info);
            this.Container.NodeID = nodeinfo.nodeID;

            Log.Debug("Main", "Found machoNet.nodeInfo, our new node id is " + nodeinfo.nodeID.ToString("X4"));
            
            // load the specified solar systems
            this.Container.SystemManager.LoadSolarSystems(nodeinfo.solarSystems);
            
            // finally set the new packet handler
            this.Socket.SetReceiveCallback(ReceiveNormalPacketCallback);
        }

        private void ReceiveNormalPacketCallback(PyObject ar)
        {
            PyPacket packet = new PyPacket();
            PyPacket res = new PyPacket();
            
            packet.Decode(ar);

            if (packet.type == Macho.MachoNetMsg_Type.CALL_REQ)
            {
                PyTuple callInfo = packet.payload.As<PyTuple>().Items[0].As<PyTuple>()[1].As<PySubStream>().Data.As<PyTuple>();

                string call = callInfo.Items[1].As<PyString>().Value;
                PyTuple args = callInfo.Items[2].As<PyTuple>();
                PyDict sub = callInfo.Items[3].As<PyDict>();
                
                Log.Trace("HandlePacket", $"Calling {packet.dest.service}::{call}");
                PyObject callResult = this.Container.ServiceManager.ServiceCall(
                    packet.dest.service, call, args, this.Container.ClientManager.Get(packet.userID)
                );

                // convert the packet to a response so we don't have to allocate a whole new packet
                res.type_string = "macho.CallRsp";
                res.type = Macho.MachoNetMsg_Type.CALL_RSP;

                // switch source and dest
                res.source = packet.dest;
                res.dest = packet.source;
                // ensure destination has clientID in it
                res.dest.typeID = packet.userID;

                res.userID = packet.userID;

                res.payload = new PyTuple();
                res.payload.Items.Add(new PySubStream(callResult));

                this.Socket.Send(res);
            }
            else if (packet.type == Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION)
            {
                Log.Debug("Main", $"Updating session for client {packet.userID}");

                // ensure the client is registered in the node and store his session
                if(this.Container.ClientManager.Contains(packet.userID) == false)
                    this.Container.ClientManager.Add(packet.userID, new Client());

                this.Container.ClientManager.Get(packet.userID).UpdateSession(packet);
            }
        }
        
        protected LowLevelVersionExchange CheckLowLevelVersionExchange(PyObject exchange)
        {
            LowLevelVersionExchange data = new LowLevelVersionExchange();

            data.Decode(exchange);

            if (data.birthday != Common.Constants.Game.birthday)
            {
                throw new Exception("Wrong birthday in LowLevelVersionExchange");
            }

            if (data.build != Common.Constants.Game.build)
            {
                throw new Exception("Wrong build in LowLevelVersionExchange");
            }

            if (data.codename != Common.Constants.Game.codename + "@" + Common.Constants.Game.region)
            {
                throw new Exception("Wrong codename in LowLevelVersionExchange");
            }

            if (data.machoVersion != Common.Constants.Game.machoVersion)
            {
                throw new Exception("Wrong machoVersion in LowLevelVersionExchange");
            }

            if (data.version != Common.Constants.Game.version)
            {
                throw new Exception("Wrong version in LowLevelVersionExchange");
            }

            if (data.isNode == true)
            {
                if (data.nodeIdentifier != "Node")
                {
                    throw new Exception("Wrong node string in LowLevelVersionExchange");
                }
            }
            
            return data;
        }
    }
}