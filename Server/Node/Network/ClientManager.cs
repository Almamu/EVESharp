using System;
using System.Collections.Generic;

namespace Node.Network
{
    /// <summary>
    /// Basic client manager to keep track of all the logged-in users in the cluster
    /// </summary>
    public class ClientManager
    {
        /// <summary>
        /// List of clients by userID
        /// </summary>
        private readonly Dictionary<long, Client> mClients = new Dictionary<long, Client>();

        /// <summary>
        /// List of clients by CharacterID
        /// </summary>
        private readonly Dictionary<int, Client> mClientsByCharacterID = new Dictionary<int, Client>();

        public EventHandler<ClientEventArgs> OnClientConnectedEvent;
        public EventHandler<ClientEventArgs> OnClientDisconnectedEvent;

        /// <summary>
        /// Adds a new client to the list
        /// </summary>
        /// <param name="userID">The id of this user</param>
        /// <param name="client">The client information</param>
        public void Add(long userID, Client client)
        {
            this.mClients.Add(userID, client);
            
            // ensure the client calls our session update handler
            client.OnSessionUpdateEvent += this.OnSessionUpdated;
        }

        /// <summary>
        /// Removes the given userID from the cluster connection list
        /// </summary>
        /// <param name="client"></param>
        public void Remove(Client client)
        {
            this.mClients.Remove(client.AccountID);
            
            if (client.CharacterID is not null)
                this.mClientsByCharacterID.Remove((int) client.CharacterID);
            
            this.OnClientDisconnectedEvent?.Invoke(this, new ClientEventArgs { Client = client });
        }

        /// <summary>
        /// Tries to get the Client instance under the given userID
        /// </summary>
        /// <param name="userID">The user to get</param>
        /// <param name="client">The output</param>
        /// <returns>Whether the client was found or not</returns>
        public bool TryGetClient(long userID, out Client client)
        {
            return this.mClients.TryGetValue(userID, out client);
        }

        /// <summary>
        /// Tries to get the Client instance under the given characterID
        /// </summary>
        /// <param name="characterID">The characterID to find</param>
        /// <param name="client">The output</param>
        /// <returns>Whether the client was found or not</returns>
        public bool TryGetClientByCharacterID(int characterID, out Client client)
        {
            return this.mClientsByCharacterID.TryGetValue(characterID, out client);
        }

        /// <summary>
        /// Handler for OnSessionUpdate on the Client
        /// </summary>
        /// <param name="client">The client that generated the event</param>
        private void OnSessionUpdated(object sender, ClientEventArgs args)
        {
            if (args.Client.CharacterID is not null)
                this.mClientsByCharacterID[(int) args.Client.CharacterID] = args.Client;
        }
    }
}