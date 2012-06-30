using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Marshal;

namespace EVESharp.ClusterControler
{
    class ConnectionManager
    {
        private static List<Connection> connections = new List<Connection>(); // This list contains ALL the connections, both nodes and clients
        private static Dictionary<int, Connection> clients = new Dictionary<int, Connection>(); // Contains the nodes list
        private static Dictionary<int, Connection> nodes = new Dictionary<int, Connection>(); // Contains the clients list

        public static int AddConnection(Socket sock)
        {
            Connection connection = new Connection(sock);

            // Priority
            connection.ClusterConnectionID = connections.Count;
            connection.Type = ConnectionType.Undefined;

            connections.Add(connection);

            return connections.Count;
        }

        public static void UpdateConnection(Connection connection)
        {
            if (connection.Type == ConnectionType.Node)
            {
                connection.NodeID = connection.ClusterConnectionID;

                if (nodes.ContainsKey(connection.NodeID))
                {
                    nodes[connection.NodeID] = connection;
                }
                else
                {
                    nodes.Add(connection.NodeID, connection);
                }
            }
            else if (connection.Type == ConnectionType.Client)
            {
                if (clients.ContainsKey(connection.AccountID))
                {
                    clients[connection.AccountID] = connection;
                }
                else
                {
                    clients.Add(connection.AccountID, connection);
                }
            }
        }

        public static void RemoveConnection(int index)
        {
            try
            {
                Connection connection = connections[index];

                if (connection == null)
                {
                    connections.RemoveAt(index);
                    return;
                }

                RemoveConnection(connection);
            }
            catch
            {

            }
        }

        public static void RemoveConnection(Connection connection)
        {
            try
            {
                if (connection.Type == ConnectionType.Client)
                {
                    clients.Remove(connection.AccountID);
                    connections.Remove(connection);
                }
                else if (connection.Type == ConnectionType.Node)
                {
                    nodes.Remove(connection.NodeID);
                    connections.Remove(connection);
                }
                else
                {
                    connections.Remove(connection);
                }
            }
            catch
            {

            }
        }

        public static Dictionary<int, Connection> Nodes
        {
            get
            {
                return nodes;
            }

            set
            {

            }
        }

        public static Dictionary<int, Connection> Clients
        {
            get
            {
                return clients;
            }

            set
            {

            }
        }

        public static void NotifyConnection(int clusterConnectionID, PyObject packet)
        {
            try
            {
                connections[clusterConnectionID].Send(packet); // This will notify the connection
            }
            catch
            {

            }
        }

        public static void NotifyClient(int accountID, PyObject packet)
        {
            try
            {
                clients[accountID].Send(packet); // This will notify the client
            }
            catch
            {

            }
        }

        public static void NotifyNode(int nodeID, PyObject packet)
        {
            try
            {
                nodes[nodeID].Send(packet);
            }
            catch
            {

            }
        }

        public static int RandomNode
        {
            get
            {
                foreach (KeyValuePair<int, Connection> node in nodes)
                {
                    return node.Key;
                }

                return 1;
            }

            private set
            {

            }
        }

        public static int ClientsCount
        {
            get
            {
                return clients.Count;
            }

            private set
            {

            }
        }

        public static int NodesCount
        {
            get
            {
                return nodes.Count;
            }

            private set
            {

            }
        }
    }
}
