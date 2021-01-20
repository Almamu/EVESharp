using System.Collections.Generic;
using Common.Services;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

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
                throw new UserError("CanOnlyDoInStations");

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
                throw new UserError("CanOnlyDoInStations");

            Station station = this.ItemManager.Stations[(int) call.Client.StationID];
            PyList result = new PyList();
            
            foreach (KeyValuePair<int, Character> pair in station.Guests)
            {
                // TODO: UPDATE WHEN FACTION WARS ARE SUPPORTED
                result.Add(new PyTuple(new PyDataType[]
                {
                    pair.Value.CharacterID, pair.Value.Corporation.ID, pair.Value.Corporation.AllianceID, 0 // facWarID
                }));
            }

            return result;
        }
    }
}