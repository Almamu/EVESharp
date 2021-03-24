using System;
using System.Runtime.InteropServices;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Notifications;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class ShipModule : ItemEntity
    {
        private readonly PyDictionary<PyInteger, PyDataType> mEffects = new PyDictionary<PyInteger, PyDataType>();
        
        public ShipModule(ItemEntity @from) : base(@from)
        {
            // ensure there's an online effect if the isOnline attribute is true
            if (this.Attributes[AttributeEnum.isOnline] == 1)
            {
                this.mEffects[16] = new PyList()
                {
                    this.ID,
                    16,
                    DateTime.UtcNow.ToFileTimeUtc(),
                    1,
                    1,
                    new PyTuple(7)
                    {
                        [0] = this.ID,
                        [1] = this.OwnerID,
                        [2] = this.LocationID,
                        [3] = null,
                        [4] = null,
                        [5] = null,
                        [6] = 16
                    },
                    0,
                    0,
                    0,
                    0,
                    null
                };
            }
        }
        
        public override PyDictionary GetEffects()
        {
            return this.mEffects;
        }

        public bool PutOffline(Client client)
        {
            // remove the effect
            this.mEffects.Remove(16);
            
            // send the OnGodmaShipEffect event to the client
            OnGodmaShipEffect effect = new OnGodmaShipEffect
            {
                Item = this,
                EffectID = 16, // online effect, hardcoded for now
                Time = DateTime.UtcNow.ToFileTimeUtc(),
                ShouldStart = 0,
                Active = 0,
                CharacterID = client.CharacterID,
                ShipID = client.ShipID,
                Target = null,
                StartTime = 0,
                Duration = 0,
                Repeat = 0,
                RandomSeed = 0
            };
            
            client.NotifyMultiEvent(effect);
            
            return true;
        }
        
        public bool PutOnline(Client client)
        {
            // store the effect in the list for this module
            this.mEffects[16] = new PyList()
            {
                this.ID,
                16,
                DateTime.UtcNow.ToFileTimeUtc(),
                1,
                1,
                new PyTuple(7)
                {
                    [0] = this.ID,
                    [1] = this.OwnerID,
                    [2] = this.LocationID,
                    [3] = null,
                    [4] = null,
                    [5] = null,
                    [6] = 16
                },
                0,
                0,
                0,
                0,
                null
            };
            
            // send the OnGodmaShipEffect event to the client
            OnGodmaShipEffect effect = new OnGodmaShipEffect
            {
                Item = this,
                EffectID = 16, // online effect, hardcoded for now
                Time = DateTime.UtcNow.ToFileTimeUtc(),
                ShouldStart = 1,
                Active = 1,
                CharacterID = client.CharacterID,
                ShipID = client.ShipID,
                Target = null,
                StartTime = 0,
                Duration = 0,
                Repeat = 0,
                RandomSeed = 0
            };
            
            client.NotifyMultiEvent(effect);

            return true;
        }
    }
}