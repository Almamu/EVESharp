using System.Collections.Generic;

namespace Node
{
    public class ClientManager
    {
        private Dictionary<long, Client> mClients = new Dictionary<long,Client>();
        
        public void Add(long userID, Client client)
        {
            this.mClients.Add(userID, client);
        }

        public Client Get(long userID)
        {
            return this.mClients[userID];
        }

        public bool Contains(long userID)
        {
            return this.mClients.ContainsKey(userID);
        }
        
        public void Remove(long userID)
        {
            this.mClients.Remove(userID);
        }
    }
}