using EVESharp.Database.Old;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;

namespace EVESharp.Node.Services.Config;

public class config : Service
{
    public override AccessLevel AccessLevel => AccessLevel.LocationPreferred;
    private         ConfigDB    DB          { get; }
    private         ILogger     Log         { get; }
    private         IItems      Items       { get; }

    public config (ConfigDB db, IItems items, ILogger log)
    {
        DB         = db;
        this.Items = items;
        Log        = log;
    }

    public PyDataType GetMultiOwnersEx (CallInformation call, PyList ids)
    {
        return DB.GetMultiOwnersEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiGraphicsEx (CallInformation call, PyList ids)
    {
        return DB.GetMultiGraphicsEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiLocationsEx (CallInformation call, PyList ids)
    {
        return DB.GetMultiLocationsEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiAllianceShortNamesEx (CallInformation call, PyList ids)
    {
        return DB.GetMultiAllianceShortNamesEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiCorpTickerNamesEx (CallInformation call, PyList ids)
    {
        return DB.GetMultiCorpTickerNamesEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMap (CallInformation call, PyInteger solarSystemID)
    {
        return DB.GetMap (solarSystemID);
    }

    // THESE PARAMETERS AREN'T REALLY USED ANYMORE, THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 1
    public PyDataType GetMapObjects (CallInformation call, PyInteger locationID, PyInteger ignored1)
    {
        return DB.GetMapObjects (locationID);
    }

    // THESE PARAMETERS AREN'T REALLY USED ANYMORE THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 0, 0, 0, 1, 0
    public PyDataType GetMapObjects
    (
        CallInformation call,        PyInteger locationID, PyInteger wantRegions, PyInteger wantConstellations,
        PyInteger       wantSystems, PyInteger wantItems,  PyInteger unknown
    )
    {
        return DB.GetMapObjects (locationID);
    }

    public PyDataType GetMapOffices (CallInformation call, PyInteger solarSystemID)
    {
        return DB.GetMapOffices (solarSystemID);
    }

    public PyDataType GetCelestialStatistic (CallInformation call, PyInteger celestialID)
    {
        if (ItemRanges.IsCelestialID (celestialID) == false)
            throw new CustomError ($"Unexpected celestialID {celestialID}");

        // TODO: CHECK FOR STATIC DATA TO FETCH IT OFF MEMORY INSTEAD OF DATABASE?
        return DB.GetCelestialStatistic (celestialID);
    }

    public PyDataType GetMultiInvTypesEx (CallInformation call, PyList typeIDs)
    {
        return DB.GetMultiInvTypesEx (typeIDs.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetStationSolarSystemsByOwner (CallInformation call, PyInteger ownerID)
    {
        return DB.GetStationSolarSystemsByOwner (ownerID);
    }

    public PyDataType GetMapConnections
    (
        CallInformation call,          PyInteger  itemID,      PyDataType isRegion, PyDataType isConstellation,
        PyDataType      isSolarSystem, PyDataType isCelestial, PyInteger  unknown2 = null
    )
    {
        bool isRegionBool        = false;
        bool isConstellationBool = false;
        bool isSolarSystemBool   = false;
        bool isCelestialBool     = false;

        if (isRegion is PyBool regionBool)
            isRegionBool = regionBool;

        if (isRegion is PyInteger regionInt)
            isRegionBool = regionInt.Value == 1;

        if (isConstellation is PyBool constellationBool)
            isConstellationBool = constellationBool;

        if (isConstellation is PyInteger constellationInt)
            isConstellationBool = constellationInt.Value == 1;

        if (isSolarSystem is PyBool solarSystemBool)
            isSolarSystemBool = solarSystemBool;

        if (isSolarSystem is PyInteger solarSystemInt)
            isSolarSystemBool = solarSystemInt.Value == 1;

        if (isCelestial is PyBool celestialBool)
            isCelestialBool = celestialBool;

        if (isCelestial is PyInteger celestialInt)
            isCelestialBool = celestialInt.Value == 1;

        if (isRegionBool)
            return DB.GetMapRegionConnection (itemID);

        if (isConstellationBool)
            return DB.GetMapConstellationConnection (itemID);

        if (isSolarSystemBool)
            return DB.GetMapSolarSystemConnection (itemID);

        if (isCelestialBool)
            Log.Error ("GetMapConnections called with celestial id. Not implemented yet!");

        return null;
    }
}