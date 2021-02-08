using System.Collections.Generic;
using Common.Services;
using Node.Exceptions;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
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

        public PyDataType GetStationItemBits(CallInformation call)
        {
            if (call.Client.StationID == null)
                throw new CanOnlyDoInStations();

            Station station = this.ItemManager.GetStation((int) call.Client.StationID);

            PyTuple result = new PyTuple(5);

            result[0] = station.StationType.HangarGraphicID;
            result[1] = station.OwnerID;
            result[2] = station.ID;
            result[3] = station.Operations.ServiceMask;
            result[4] = station.Type.ID;

            return result;
        }

        public PyDataType GetGuests(CallInformation call)
        {
            if (call.Client.StationID == null)
                throw new CanOnlyDoInStations();

            Station station = this.ItemManager.GetStation((int) call.Client.StationID);
            PyList result = new PyList();
            
            foreach (KeyValuePair<int, Character> pair in station.Guests)
            {
                // TODO: UPDATE WHEN FACTION WARS ARE SUPPORTED
                result.Add(new PyTuple(4)
                    {
                        [0] = pair.Value.CharacterID,
                        [1] = pair.Value.Corporation.ID,
                        [2] = pair.Value.Corporation.AllianceID,
                        [3] = 0 // facWarID
                    }
                );
            }

            return result;
        }
    }
}