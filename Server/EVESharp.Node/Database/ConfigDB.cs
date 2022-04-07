using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class ConfigDB : DatabaseAccessor
{
    private NodeContainer NodeContainer { get; }

    public ConfigDB (NodeContainer nodeContainer, DatabaseConnection db) : base (db)
    {
        NodeContainer = nodeContainer;
    }

    /// <summary>
    /// Obtains a list of owners ready for the EVE Client
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public PyDataType GetMultiOwnersEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT itemID as ownerID, itemName as ownerName, typeID FROM eveNames WHERE itemID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return TupleSet.FromMySqlDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains a list of graphics ready for the EVE Client
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public PyDataType GetMultiGraphicsEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT graphicID, url3D, urlWeb, icon, urlSound, explosionID FROM eveGraphics WHERE graphicID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return TupleSet.FromMySqlDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains a list of locations ready for the EVE Client
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public PyDataType GetMultiLocationsEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT itemID as locationID, itemName as locationName, x, y, z FROM invItems LEFT JOIN eveNames USING(itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return TupleSet.FromMySqlDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains a list of alliance shortnames ready for the EVE Client
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public PyDataType GetMultiAllianceShortNamesEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT allianceID, shortName FROM crpAlliances WHERE allianceID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return TupleSet.FromMySqlDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains a list of corp tickers ready for the EVE Client
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public PyDataType GetMultiCorpTickerNamesEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT corporationID, tickerName, shape1, shape2, shape3, color1, color2, color3 FROM corporation WHERE corporationID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return TupleSet.FromMySqlDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Gets map information for the given <paramref name="solarSystemID"/>
    /// </summary>
    /// <param name="solarSystemID"></param>
    /// <returns></returns>
    public Rowset GetMap (int solarSystemID)
    {
        Rowset result = Database.PrepareRowsetQuery (
            "SELECT IF(groupID = 10, (SELECT GROUP_CONCAT(celestialID SEPARATOR ',') FROM mapJumps WHERE stargateID = itemID), NULL) AS destinations, itemID, itemName, typeID, mapDenormalize.x, mapDenormalize.y, mapDenormalize.z, xMin, yMin, zMin, xMax, yMax, zMax, orbitID, luminosity, mapDenormalize.solarSystemID AS locationID FROM mapDenormalize LEFT JOIN mapSolarSystems ON mapSolarSystems.solarSystemID = mapDenormalize.itemID WHERE mapDenormalize.solarSystemID = @solarSystemID",
            new Dictionary <string, object> {{"@solarSystemID", solarSystemID}}
        );

        // iterate all the results and create the list based on the concatenated column
        foreach (PyList line in result.Rows)
        {
            // get destinations string
            PyDataType firstItem = line [0];

            // ignore null entries
            if (firstItem is PyString == false)
                continue;

            PyString destinations = firstItem as PyString;

            // get the list of ids
            string [] list = destinations.Value.Split (',');

            PyList destinationsList = new PyList (list.Length);

            int i = 0;

            // put the ids in the new destination list
            foreach (string id in list)
                destinationsList [i++] = int.Parse (id);

            // finally store it in the line
            line [0] = destinationsList;
        }

        // return the result!
        return result;
    }

    /// <summary>
    /// Get map objects for the given location <paramref name="itemID"/>
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public CRowset GetMapObjects (int itemID)
    {
        if (itemID == NodeContainer.Constants [Constants.locationUniverse])
            return Database.PrepareCRowsetQuery (
                $"SELECT groupID, typeID, itemID, itemName, {itemID} as locationID, orbitID, 0 AS connection, x, y, z FROM mapDenormalize WHERE typeID = 3"
            );

        return Database.PrepareCRowsetQuery (
            "SELECT groupID, typeID, itemID, itemName, solarSystemID AS locationID, orbitID, 0 AS connection, x, y, z FROM mapDenormalize WHERE solarSystemID = @solarSystemID",
            new Dictionary <string, object> {{"@solarSystemID", itemID}}
        );
    }

    /// <summary>
    /// Gets the offices on the given solarSystemID
    /// </summary>
    /// <param name="solarSystemID"></param>
    /// <returns></returns>
    public Rowset GetMapOffices (int solarSystemID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT crpOffices.corporationID, crpOffices.stationID FROM crpOffices, staStations WHERE crpOffices.stationID = staStations.stationID AND staStations.solarSystemID = @solarSystemID",
            new Dictionary <string, object> {{"@solarSystemID", solarSystemID}}
        );
    }

    /// <summary>
    /// Gets information about the given <paramref name="celestialID"/>
    /// </summary>
    /// <param name="celestialID"></param>
    /// <returns></returns>
    public CRowset GetCelestialStatistic (int celestialID)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT temperature, spectralClass, luminosity, age, life, orbitRadius, eccentricity, massDust, massGas, fragmented, density, surfaceGravity, escapeVelocity, orbitPeriod, rotationRate, locked, pressure, radius, mass FROM mapCelestialStatistics WHERE celestialID = @celestialID",
            new Dictionary <string, object> {{"@celestialID", celestialID}}
        );
    }

    public PyDataType GetMultiInvTypesEx (PyList <PyInteger> ids)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT typeID, groupID, typeName, description, graphicID, radius, mass, volume, capacity, portionSize, raceID, basePrice, published, marketGroupID, chanceOfDuplicating, dataID FROM invTypes WHERE typeID IN ({PyString.Join (',', ids)})"
        );

        using (connection)
        using (reader)
        {
            return RowList.FromMySqlDataReader (Database, reader);
        }
    }

    public Rowset GetStationSolarSystemsByOwner (int ownerID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT corporationID, solarSystemID FROM staStations WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", ownerID}}
        );
    }

    public PyList <PyTuple> GetMapRegionConnection (int universeID)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT origin.regionID AS fromRegionID, origin.constellationID AS fromConstellationID, origin.solarSystemID AS fromSolarSystemID, stargateID, celestialID, destination.solarSystemID AS toSolarSystemID, destination.constellationID AS toConstellationID, destination.regionID AS toRegionID FROM mapJumps LEFT JOIN mapDenormalize origin ON origin.itemID = mapJumps.stargateID LEFT JOIN mapDenormalize destination ON destination.itemID = mapJumps.celestialID",
            new Dictionary <string, object> {{"@locationID", universeID}}
        );

        using (connection)
        using (reader)
        {
            PyList <PyTuple> result = new PyList <PyTuple> ();

            while (reader.Read ())
                result.Add (
                    new PyTuple (9)
                    {
                        [0] = "",
                        [1] = reader.GetInt32 (0),
                        [2] = reader.GetInt32 (1),
                        [3] = reader.GetInt32 (2),
                        [4] = reader.GetInt32 (3),
                        [5] = reader.GetInt32 (4),
                        [6] = reader.GetInt32 (5),
                        [7] = reader.GetInt32 (6),
                        [8] = reader.GetInt32 (7)
                    }
                );

            return result;
        }
    }

    public PyList <PyTuple> GetMapConstellationConnection (int regionID)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT fromRegionID, fromConstellationID, toConstellationID, toRegionID FROM mapConstellationJumps WHERE fromRegionID = @locationID",
            new Dictionary <string, object> {{"@locationID", regionID}}
        );

        using (connection)
        using (reader)
        {
            PyList <PyTuple> result = new PyList <PyTuple> ();

            while (reader.Read ())
                result.Add (
                    new PyTuple (9)
                    {
                        [0] = "",
                        [1] = reader.GetInt32 (0),
                        [2] = reader.GetInt32 (1),
                        [3] = 0,
                        [4] = 0,
                        [5] = 0,
                        [6] = 0,
                        [7] = reader.GetInt32 (2),
                        [8] = reader.GetInt32 (3)
                    }
                );

            return result;
        }
    }

    public PyList <PyTuple> GetMapSolarSystemConnection (int constellationID)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT fromRegionID, fromConstellationID, fromSolarSystemID, toSolarSystemID, toConstellationID, toRegionID FROM mapSolarSystemJumps WHERE fromConstellationID = @locationID",
            new Dictionary <string, object> {{"@locationID", constellationID}}
        );

        using (connection)
        using (reader)
        {
            PyList <PyTuple> result = new PyList <PyTuple> ();

            while (reader.Read ())
                result.Add (
                    new PyTuple (9)
                    {
                        [0] = "",
                        [1] = reader.GetInt32 (0),
                        [2] = reader.GetInt32 (1),
                        [3] = reader.GetInt32 (2),
                        [4] = 0,
                        [5] = 0,
                        [6] = reader.GetInt32 (3),
                        [7] = reader.GetInt32 (4),
                        [8] = reader.GetInt32 (5)
                    }
                );

            return result;
        }
    }
}