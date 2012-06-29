using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;

namespace Proxy
{
    public static class ClientManager
    {
        private static List<Client> clients = new List<Client>();
        public static int GetClientsCount()
        {
            return clients.Count;
        }

        public static int GetClientID(Client cli)
        {
            int clientID = 0;
            foreach (Client client in clients)
            {
                if (cli.GetAccountID() == client.GetAccountID())
                {
                    return clientID;
                }

                clientID++;
            }

            return 0;
        }

        public static Client GetClient(int clientID)
        {
            try
            {
                return clients[clientID];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int AddClient(Client client)
        {
            int clientID = clients.Count;

            clients.Add(client);

            return clientID;
        }

        public static bool RemoveClient(Client client)
        {
            if (client == null)
            {
                return false;
            }

            return clients.Remove(client);
        }

        public static bool RemoveClient(int clientID)
        {
            if (clientID < 0)
            {
                Log.Debug("ClientManager", "Got invalid clientID in RemoveClient");
                return false;
            }

            try
            {
                clients.RemoveAt(clientID);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }

        public static void NotifyClients(PyObject notify)
        {
            foreach (Client client in clients)
            {
                client.Send(notify);
            }
        }

        public static void NotifyClient(int clientID, PyObject notify)
        {
            try
            {
                GetClient(clientID).Send(notify);
            }
            catch (Exception)
            {

            }
        }
    }
}
