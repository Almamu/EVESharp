using System.Collections.Generic;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class station : Service
    {
        public station(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetStationItemBits(PyDictionary namedPayload, Client client)
        {
            if (client.StationID == null)
                throw new UserError("CanOnlyDoInStations");

            Station station =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.StationID) as Station;

            PyTuple result = new PyTuple(5);

            result[0] = station.StationType.HangarGraphicID;
            result[1] = station.OwnerID;
            result[2] = station.ID;
            result[3] = station.Operations.ServiceMask;
            result[4] = station.Type.ID;

            return result;
        }

        public PyDataType GetGuests(PyDictionary namedPayload, Client client)
        {
            if (client.StationID == null)
                throw new UserError("CanOnlyDoInStations");

            Station station = this.ServiceManager.Container.ItemFactory.ItemManager.Stations[(int) client.StationID];
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