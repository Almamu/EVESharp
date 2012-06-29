using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Common.Network;
using Common.Packets;
using Marshal;

namespace Proxy
{
    // Carefull, the proxy is the nodeID 1, so we should add/sub 2 to every nodeID
    public static class NodeManager
    {
        public enum NodeStatus
        {
            NodeAlive = 0,
            NodeUnderHighLoad = 1,
            NodeWaitingHeartbeat = 2,
        }

        public static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        private static Dictionary<int, NodeStatus> nodeStatus = new Dictionary<int, NodeStatus>();

        public static int AddNode(Node node)
        {
            int nodeID = nodes.Count;

            // Add the nodes
            nodeStatus.Add(nodeID, NodeStatus.NodeAlive);
            nodes.Add(nodeID, node);

            if (node == null)
            {
                Log.Debug("NodeManager", "Adding dummy node with id " + nodeID);
                return 0; // Ignore it...
            }

            Log.Debug("Node", "Node connected with id " + nodeID);

            // Send a notification with the nodeInfo
            NodeInfo info = new NodeInfo();

            info.nodeID = nodeID;
            info.solarSystems.Items.Add(new PyNone()); // This will let the node load the solarSystems it wants

            node.Notify(info.Encode());

            return nodeID;
        }

        public static int GetRandomNode()
        {
            bool found = false;
            int nodeID = 0;

            if (nodes.Count == 0)
            {
                return nodes.Count;
            }

            Random rnd = new Random();

            while (found == false)
            {
                Thread.Sleep(1);

                nodeID = rnd.Next(nodes.Count);

                try
                {
                    if (nodes[nodeID] == null)
                    {
                        continue;
                    }

                    if (nodeStatus[nodeID] == NodeStatus.NodeAlive)
                    {
                        return nodeID;
                    }
                }
                catch (Exception)
                {
                    Log.Error("NodeManager", "Node id " + nodeID + " doesnt has a status assigned to it...");
                }
            }

            return nodeID;
        }

        public static int GetNodeID(Node node)
        {
            int nodeID = 0;

            foreach (Node itnode in nodes.Values)
            {
                if (itnode == node)
                {
                    return nodeID;
                }

                nodeID++;
            }

            return -1;
        }

        public static bool IsNodeUnderHighLoad(int nodeID)
        {
            return nodeStatus[nodeID] == NodeStatus.NodeUnderHighLoad;
        }

        public static void HeartbeatNodes()
        {
            // Check if an older heartbeat left a node waiting
            foreach (int nodeID in nodeStatus.Keys)
            {
                if (nodes[nodeID] == null)
                {
                    continue;
                }

                if (nodeStatus[nodeID] == NodeStatus.NodeWaitingHeartbeat)
                {
                    // Node dead, load all the solarSystems into other node and transfer the clients
                    try
                    {
                        nodes.Remove(nodeID);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            PyTuple data = new PyTuple();

            data.Items.Add(new PyLongLong(DateTime.Now.ToFileTime()));

            NotifyNodes(new PyObjectData("macho.PingReq", data));
        }

        public static void NotifyNodes(PyObject notify)
        {
            foreach (int nodeID in nodes.Keys)
            {
                NotifyNode(nodeID, notify);
            }
        }

        public static bool NotifyNode(int nodeID, PyObject data)
        {
            try
            {
                if (nodes[nodeID] == null)
                {
                    return false; // We receive a packet in a dummy node, return false
                }

                nodes[nodeID].Notify(data);
                return true;
            }
            catch (Exception)
            {
                Log.Error("NodeManager", "Trying to send data to a non-existing node");
                Log.Warning("NodeManager", PrettyPrinter.Print(data));
                return false;
            }
        }

        public static void RemoveNode(int nodeID)
        {
            nodes.Remove(nodeID);
            nodeStatus.Remove(nodeID);
        }

        public static void RemoveNode(Node node)
        {
            RemoveNode(GetNodeID(node));
        }
    }
}
