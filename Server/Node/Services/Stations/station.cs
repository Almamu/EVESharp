using System.Collections.Generic;
using Common.Services;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class station : Service
    {
        private ItemManager ItemManager { get; }
        public station(ItemManager itemManager) 
        {
            this.ItemManager = itemManager;
        }

        public PyTuple GetStationItemBits(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();

            Station station = this.ItemManager.GetStation(stationID);

            return new PyTuple(5)
            {
                [0] = station.StationType.HangarGraphicID,
                [1] = station.OwnerID,
                [2] = station.ID,
                [3] = station.Operations.ServiceMask,
                [4] = station.Type.ID
            };
        }

        public PyList<PyTuple> GetGuests(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();

            Station station = this.ItemManager.GetStation(stationID);
            PyList<PyTuple> result = new PyList<PyTuple>();
            
            foreach ((int _, Character character) in station.Guests)
            {
                // TODO: UPDATE WHEN FACTION WARS ARE SUPPORTED
                result.Add(
                    new PyTuple(4)
                    {
                        [0] = character.CharacterID,
                        [1] = character.Corporation.ID,
                        [2] = character.Corporation.AllianceID,
                        [3] = 0 // facWarID
                    }
                );
            }

            return result;
        }
    }
}