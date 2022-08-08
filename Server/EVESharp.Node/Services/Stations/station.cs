using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Services.Stations;

public class station : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         IItems Items { get; }

    public station (IItems items)
    {
        this.Items = items;
    }

    [MustBeInStation]
    public PyTuple GetStationItemBits (CallInformation call)
    {
        int stationID = call.Session.StationID;

        Station station = this.Items.GetStaticStation (stationID);

        return new PyTuple (5)
        {
            [0] = station.StationType.HangarGraphicID,
            [1] = station.OwnerID,
            [2] = station.ID,
            [3] = station.Operations.ServiceMask,
            [4] = station.Type.ID
        };
    }

    [MustBeInStation]
    public PyList <PyTuple> GetGuests (CallInformation call)
    {
        int stationID = call.Session.StationID;

        Station          station = this.Items.GetStaticStation (stationID);
        PyList <PyTuple> result  = new PyList <PyTuple> ();

        foreach ((int _, Character character) in station.Guests)
            // TODO: UPDATE WHEN FACTION WARS ARE SUPPORTED
            result.Add (
                new PyTuple (4)
                {
                    [0] = character.ID,
                    [1] = character.CorporationID,
                    [2] = character.AllianceID,
                    [3] = 0 // facWarID
                }
            );

        return result;
    }
}