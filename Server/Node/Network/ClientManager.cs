using System;
using System.Collections.Generic;
using PythonTypes.Types.Primitives;

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

        /// <summary>
        /// List of clients by CorporationID
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, Client>> mClientsByCorporationID = new Dictionary<int, Dictionary<int, Client>>();

        /// <summary>
        /// Event handler for when a client selects a character
        /// </summary>
        public event EventHandler<ClientEventArgs> OnCharacterSelectedEvent;
        
        /// <summary>
        /// Adds a new client to the list
        /// </summary>
        /// <param name="userID">The id of this user</param>
        /// <param name="client">The client information</param>
        public void Add(long userID, Client client)
        {
            this.mClients.Add(userID, client);
            
            // bind some of the client's events
            client.OnSessionUpdateEvent += this.OnSessionUpdated;
            client.OnCharacterSelectedEvent += this.OnCharacterSelected;
        }

        /// <summary>
        /// Removes the given userID from the cluster connection list
        /// </summary>
        /// <param name="client"></param>
        public void Remove(Client client)
        {
            this.mClients.Remove(client.AccountID);

            // if a character was selected search for it in the cache lists
            // and ensure they're removed
            if (client.CharacterID is not null)
            {
                this.mClientsByCharacterID.Remove((int) client.CharacterID);
                
                if (this.mClientsByCorporationID.TryGetValue(client.CorporationID, out Dictionary<int, Client> characterList) == true) 
                    characterList.Remove((int) client.CharacterID);
            }


            // remove the events listener from the client
            client.OnSessionUpdateEvent -= this.OnSessionUpdated;
            client.OnCharacterSelectedEvent -= this.OnCharacterSelected;
            
            client.OnClientDisconnected();
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
        /// Checks if the given characterID registered in the client manager
        /// </summary>
        /// <param name="characterID"></param>
        /// <returns>Whether the client was found or not</returns>
        public bool ContainsCharacterID(int characterID)
        {
            return this.mClientsByCharacterID.ContainsKey(characterID);
        }

        /// <summary>
        /// Tries to get the Clients list under the given corporationID
        /// </summary>
        /// <param name="corporationID">The corporationID to find</param>
        /// <param name="clients">The list of clients</param>
        /// <returns>Whether there's any or not</returns>
        public bool TryGetClientsByCorporationID(int corporationID, out Dictionary<int, Client> clients)
        {
            return this.mClientsByCorporationID.TryGetValue(corporationID, out clients);
        }
        
        /// <summary>
        /// Handler for OnSessionUpdate on the Client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSessionUpdated(object sender, ClientSessionEventArgs args)
        {
            // TODO: HANDLE THIS WITH THE MACHONET CORPORATION MEMBER CHANGED EVENT INSTEAD
            // update corporation lists
            int? oldCorporationID = args.Session.GetPrevious("corpid") as PyInteger;
            int? newCorporationID = args.Session.GetCurrent("corpid") as PyInteger;

            if (newCorporationID is not null)
            {
                if (this.mClientsByCorporationID.TryGetValue((int) newCorporationID, out Dictionary<int, Client> clients) == false)
                    clients = this.mClientsByCorporationID[(int) newCorporationID] = new Dictionary<int, Client>();

                lock (clients)
                    clients.Add((int) args.Client.CharacterID, args.Client);
            
                // this lookup won't fail as long as there was a corporationID, so no need to do a tryGetValue
                if (oldCorporationID is not null)
                {
                    clients = this.mClientsByCorporationID[(int) oldCorporationID];
                    clients.Remove((int) args.Client.CharacterID);
                }
            }
        }

        private void OnCharacterSelected(object sender, ClientEventArgs args)
        {
            this.mClientsByCharacterID.Add((int) args.Client.CharacterID, args.Client);

            // notify services about this
            this.OnCharacterSelectedEvent?.Invoke(this, args);
        }
    }
}