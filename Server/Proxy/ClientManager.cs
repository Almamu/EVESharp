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
        public static int GetClientID(Client cli)
        {
            int clientID = 0;
            foreach (Client client in Program.clients)
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
                return Program.clients[clientID];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int AddClient(Client client)
        {
            int clientID = Program.clients.Count;

            Program.clients.Add(client);

            return clientID;
        }

        public static bool RemoveClient(Client client)
        {
            if (client == null)
            {
                return false;
            }

            return Program.clients.Remove(client);
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
                Program.clients.RemoveAt(clientID);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }

        public static void NotifyClients(PyObject notify)
        {
            foreach (Client client in Program.clients)
            {
                client.Send(notify);
            }
        }
    }
}
