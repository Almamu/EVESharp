using System.Collections.Generic;

namespace Node
{
    /// <summary>
    /// Basic client manager to keep track of all the logged-in users in the cluster
    /// </summary>
    public class ClientManager
    {
        private readonly Dictionary<long, Client> mClients = new Dictionary<long, Client>();

        /// <summary>
        /// Adds a new client to the list
        /// </summary>
        /// <param name="userID">The id of this user</param>
        /// <param name="client">The client information</param>
        public void Add(long userID, Client client)
        {
            this.mClients.Add(userID, client);
        }

        /// <param name="userID">The id of the user to get</param>
        /// <returns>The client information</returns>
        public Client Get(long userID)
        {
            return this.mClients[userID];
        }

        /// <summary>
        /// Checks if an user is present in the cluster
        /// </summary>
        /// <param name="userID">The id of the user</param>
        /// <returns>If the user is connected or not</returns>
        public bool Contains(long userID)
        {
            return this.mClients.ContainsKey(userID);
        }

        /// <summary>
        /// Removes the given userID from the cluster connection list
        /// </summary>
        /// <param name="userID"></param>
        public void Remove(long userID)
        {
            this.mClients.Remove(userID);
        }
    }
}