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
        private static List<Connection> nodes = new List<Connection>(); // Contains the nodes list
        private static List<Connection> clients = new List<Connection>(); // Contains the clients list

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
                nodes.Add(connection);

                connection.NodeID = connection.ClusterConnectionID;
            }
            else if (connection.Type == ConnectionType.Client)
            {
                clients.Add(connection);
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
                    clients.Remove(connection);
                    connections.Remove(connection);
                }
                else if (connection.Type == ConnectionType.Node)
                {
                    nodes.Remove(connection);
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

        public static List<Connection> GetNodeList()
        {
            return nodes;
        }

        public static List<Connection> GetClientList()
        {
            return clients;
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

        public static int RandomNode
        {
            get
            {
                foreach (Connection node in nodes)
                {
                    return node.NodeID;
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
