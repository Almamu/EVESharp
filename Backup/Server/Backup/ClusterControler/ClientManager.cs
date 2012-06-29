using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace EVESharp.ClusterControler
{
    class ClientManager
    {
        private static List<Client> clients = new List<Client>();

        public static int AddClient(Socket sock)
        {
            Client client = new Client(sock);

            clients.Add(client);

            client.ClientID = clients.Count;

            return clients.Count;
        }

        public static void RemoveClient(int index)
        {
            try
            {
                clients.RemoveAt(index);
            }
            catch
            {

            }
        }
    }
}
