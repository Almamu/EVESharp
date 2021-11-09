using System.Collections.Generic;
using EVESharp.Common.Services;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Stations
{
    public class station : IService
    {
        private ItemFactory ItemFactory { get; }
        public station(ItemFactory itemFactory) 
        {
            this.ItemFactory = itemFactory;
        }

        public PyTuple GetStationItemBits(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();

            Station station = this.ItemFactory.GetStaticStation(stationID);

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

            Station station = this.ItemFactory.GetStaticStation(stationID);
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