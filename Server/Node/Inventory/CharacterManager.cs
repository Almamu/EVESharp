using System.Collections.Generic;
using Node.Network;

namespace Node.Inventory
{
    public class CharacterManager
    {
        private readonly Dictionary<int, Client> mCharacters = new Dictionary<int, Client>();

        public void AddCharacter(int characterID, Client client)
        {
            this.mCharacters[characterID] = client;
        }

        public void RemoveCharacter(int characterID)
        {
            this.mCharacters.Remove(characterID);
        }

        public bool IsCharacterConnected(int characterID)
        {
            return this.mCharacters.ContainsKey(characterID);
        }
    }
}