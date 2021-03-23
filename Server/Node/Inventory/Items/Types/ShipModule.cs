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

        public bool IsOnline()
        {
            return this.Attributes[AttributeEnum.isOnline] == 1;
        }

        public bool PutOffline(Client client)
        {
            if (this.IsOnline() == false)
                return true;
            
            // reduce the cpu and power load on the ship
            Ship ship = this.ItemFactory.ItemManager.GetItem<Ship>(this.LocationID);

            if (ship is null)
                return false;

            if (this.Attributes.TryGetAttribute(AttributeEnum.cpu, out ItemAttribute cpu) == true)
                client.NotifyAttributeChange(ship.CPULoad -= cpu, ship);

            if (this.Attributes.TryGetAttribute(AttributeEnum.power, out ItemAttribute power) == true)
                client.NotifyAttributeChange(ship.PowerLoad -= power, ship);
            
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
            
            // mark the module as not online
            this.Attributes[AttributeEnum.isOnline].Integer = 0;
            
            return true;
        }
        
        public bool PutOnline(Client client)
        {
            // only singletons can be put online
            if (this.Singleton == false)
                return false;
            if (this.IsOnline() == true)
                return true;
            
            // ensure the character has the required skills
            this.CheckPrerequisites(this.ItemFactory.ItemManager.GetItem<Character>(client.EnsureCharacterIsSelected()));
            
            // perform pre-requirement checks
            Ship ship = this.ItemFactory.ItemManager.GetItem<Ship>(this.LocationID);
            
            // ensure the parent has enough resources available
            if (ship is null)
                return false;

            ItemAttribute cpuLoad = ship.CPULoad;
            ItemAttribute cpu = this.Attributes[AttributeEnum.cpu];

            // TODO: SEND THE PLAYER MESSAGES ABOUT WHY THIS CANNOT BE DONE
            if (cpu + cpuLoad > ship.CPUOutput)
                return false;

            ItemAttribute powerLoad = ship.PowerLoad;
            ItemAttribute power = this.Attributes[AttributeEnum.power];
            
            if (power + powerLoad > ship.PowerOutput)
                return false;
            // TODO: CHECK FOR IN-SPACE AND ENSURE THERE'S AT LEAST 75% OF CAP AVAILABLE

            // increase cpu and power load in the ship
            powerLoad = ship.PowerLoad += power;
            cpuLoad = ship.CPULoad += cpu;
            
            // notify owner of the changes in the attributes
            client.NotifyAttributeChange(powerLoad, ship);
            client.NotifyAttributeChange(cpuLoad, ship);

            // finally set the module online
            this.Attributes[AttributeEnum.isOnline].Integer = 1;
            
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
            
            client.NotifyAttributeChange(this.Attributes[AttributeEnum.isOnline], this);
            
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