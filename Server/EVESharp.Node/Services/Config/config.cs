using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Services.Config;

public class config : Service
{
    public override AccessLevel AccessLevel => AccessLevel.LocationPreferred;
    private         ConfigDB    DB          { get; }
    private         ILogger     Log         { get; }
    private         ItemFactory ItemFactory { get; }

    public config (ConfigDB db, ItemFactory itemFactory, ILogger log)
    {
        DB          = db;
        ItemFactory = itemFactory;
        Log         = log;
    }

    public PyDataType GetMultiOwnersEx (PyList ids, CallInformation call)
    {
        return DB.GetMultiOwnersEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiGraphicsEx (PyList ids, CallInformation call)
    {
        return DB.GetMultiGraphicsEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiLocationsEx (PyList ids, CallInformation call)
    {
        return DB.GetMultiLocationsEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiAllianceShortNamesEx (PyList ids, CallInformation call)
    {
        return DB.GetMultiAllianceShortNamesEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMultiCorpTickerNamesEx (PyList ids, CallInformation call)
    {
        return DB.GetMultiCorpTickerNamesEx (ids.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetMap (PyInteger solarSystemID, CallInformation call)
    {
        return DB.GetMap (solarSystemID);
    }

    // THESE PARAMETERS AREN'T REALLY USED ANYMORE, THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 1
    public PyDataType GetMapObjects (PyInteger locationID, PyInteger ignored1, CallInformation call)
    {
        return DB.GetMapObjects (locationID);
    }

    // THESE PARAMETERS AREN'T REALLY USED ANYMORE THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 0, 0, 0, 1, 0
    public PyDataType GetMapObjects (
        PyInteger locationID,  PyInteger wantRegions, PyInteger wantConstellations,
        PyInteger wantSystems, PyInteger wantItems,   PyInteger unknown, CallInformation call
    )
    {
        return DB.GetMapObjects (locationID);
    }

    public PyDataType GetMapOffices (PyInteger solarSystemID, CallInformation call)
    {
        return DB.GetMapOffices (solarSystemID);
    }

    public PyDataType GetCelestialStatistic (PyInteger celestialID, CallInformation call)
    {
        if (ItemRanges.IsCelestialID (celestialID) == false)
            throw new CustomError ($"Unexpected celestialID {celestialID}");

        // TODO: CHECK FOR STATIC DATA TO FETCH IT OFF MEMORY INSTEAD OF DATABASE?
        return DB.GetCelestialStatistic (celestialID);
    }

    public PyDataType GetMultiInvTypesEx (PyList typeIDs, CallInformation call)
    {
        return DB.GetMultiInvTypesEx (typeIDs.GetEnumerable <PyInteger> ());
    }

    public PyDataType GetStationSolarSystemsByOwner (PyInteger ownerID, CallInformation call)
    {
        return DB.GetStationSolarSystemsByOwner (ownerID);
    }

    public PyDataType GetMapConnections (
        PyInteger  itemID,        PyDataType isRegion,    PyDataType isConstellation,
        PyDataType isSolarSystem, PyDataType isCelestial, PyInteger  unknown2 = null, CallInformation call = null
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