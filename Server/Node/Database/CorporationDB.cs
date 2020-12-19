using Common.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class CorporationDB : DatabaseAccessor
    {
        public CorporationDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType ListAllCorpFactions()
        {
            return Database.PrepareIntIntDictionary("SELECT corporationID, factionID from crpNPCCorporations");
        }

        public PyDataType ListAllFactionStationCount()
        {
            return Database.PrepareIntIntDictionary("SELECT factionID, COUNT(stationID) FROM crpNPCCorporations LEFT JOIN staStations USING (corporationID) GROUP BY factionID");
        }

        public PyDataType ListAllFactionSolarSystemCount()
        {
            return Database.PrepareIntIntDictionary("SELECT factionID, COUNT(solarSystemID) FROM crpNPCCorporations GROUP BY factionID");
        }

        public PyDataType ListAllFactionRegions()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, regionID FROM mapRegions WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDataType ListAllFactionConstellations()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, constellationID FROM mapConstellations WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDataType ListAllFactionSolarSystems()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, solarSystemID FROM mapSolarSystems WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDataType ListAllFactionRaces()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, raceID FROM factionRaces WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDataType ListAllNPCCorporationInfo()
        {
            return Database.PrepareIntRowDictionary(
                "SELECT " +
                "   corporationID," +
                "   corporationName, mainActivityID, secondaryActivityID," +
                "   size, extent, solarSystemID, investorID1, investorShares1," +
                "   investorID2, investorShares2, investorID3, investorShares3," +
                "   investorID4, investorShares4," +
                "   friendID, enemyID, publicShares, initialPrice," +
                "   minSecurity, scattered, fringe, corridor, hub, border," +
                "   factionID, sizeFactor, stationCount, stationSystemCount," +
                "   stationID, ceoID, entity.itemName AS ceoName" +
                " FROM crpNPCCorporations" +
                " JOIN corporation USING (corporationID)" +
                "   LEFT JOIN entity ON ceoID=entity.itemID", 0
            );
        }
    }
}