using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
