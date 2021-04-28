using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class CorporationDB : DatabaseAccessor
    {
        public PyDictionary<PyInteger,PyInteger> ListAllCorpFactions()
        {
            return Database.PrepareIntIntDictionary("SELECT corporationID, factionID from crpNPCCorporations");
        }

        public PyDictionary<PyInteger,PyInteger> ListAllFactionStationCount()
        {
            return Database.PrepareIntIntDictionary("SELECT factionID, COUNT(stationID) FROM crpNPCCorporations LEFT JOIN staStations USING (corporationID) GROUP BY factionID");
        }

        public PyDictionary<PyInteger,PyInteger> ListAllFactionSolarSystemCount()
        {
            return Database.PrepareIntIntDictionary("SELECT factionID, COUNT(solarSystemID) FROM crpNPCCorporations GROUP BY factionID");
        }

        public PyDictionary<PyInteger,PyList<PyInteger>> ListAllFactionRegions()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, regionID FROM mapRegions WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDictionary<PyInteger,PyList<PyInteger>> ListAllFactionConstellations()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, constellationID FROM mapConstellations WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDictionary<PyInteger,PyList<PyInteger>> ListAllFactionSolarSystems()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, solarSystemID FROM mapSolarSystems WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDictionary<PyInteger,PyList<PyInteger>> ListAllFactionRaces()
        {
            return Database.PrepareIntIntListDictionary("SELECT factionID, raceID FROM factionRaces WHERE factionID IS NOT NULL ORDER BY factionID");
        }

        public PyDictionary ListAllNPCCorporationInfo()
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
                "   stationID, ceoID, eveNames.itemName AS ceoName" +
                " FROM crpNPCCorporations" +
                " JOIN corporation USING (corporationID)" +
                "   LEFT JOIN eveNames ON ceoID = eveNames.itemID", 0
            );
        }

        public Rowset GetEveOwners(int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT characterID as ownerID, itemName AS ownerName, typeID FROM chrInformation, eveNames WHERE eveNames.itemID = chrInformation.characterID AND corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public Rowset GetNPCDivisions()
        {
            return Database.PrepareRowsetQuery(
                "SELECT divisionID, divisionName, description, leaderType from crpNPCDivisions"
            );
        }

        public Rowset GetSharesByShareholder(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT corporationID, shares FROM crpCharShares WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetMedalsReceived(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT medalID, title, description, ownerID, issuerID, date, reason, status FROM chrMedals WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetEmploymentRecord(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT corporationID, startDate, deleted FROM chrEmployment WHERE characterID=@characterID ORDER BY startDate DESC",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public double GetLPForCharacterCorp(int corporationID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT balance FROM chrLPbalance WHERE characterID=@characterID AND corporationID=@corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                // no records means the character doesn't have any LP with the corp yet
                if (reader.Read() == false)
                    return 0.0f;

                return reader.GetDouble(0);
            }
        }

        public PyDataType GetMember(int characterID, int corporationID)
        {
            // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
            // TODO: titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther
            // TODO: divisionID, squadronID
            // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " characterID, title, startDateTime, corpRole AS roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
                " titleMask, 0 AS grantableRoles, 0 AS grantableRolesAtHQ, 0 AS grantableRolesAtBase," +
                " 0 AS grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, locationID AS baseID, " +
                " 0 AS blockRoles, gender " +
                "FROM chrInformation " +
                "LEFT JOIN invItems ON invItems.itemID = chrInformation.activeCloneID " +
                "WHERE corporationID=@corporationID AND characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID}
                }
            );
        }

        public Dictionary<PyDataType, int> GetOffices(int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemID FROM crpOffices WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
            
            using (connection)
            using (reader)
            {
                Dictionary<PyDataType, int> result = new Dictionary<PyDataType, int>();
                int index = 0;
                
                while (reader.Read() == true)
                {
                    result[reader.GetInt32(0)] = index ++;
                }

                return result;
            }
        }

        public PyList<PyTuple> GetOffices(PyList<PyInteger> itemIDs, int corporationID, SparseRowsetHeader header, Dictionary<PyDataType, int> rowsIndex)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            
            string query = "SELECT" +
                           " itemID, stationID, typeID, officeFolderID " +
                           "FROM chrInformation " +
                           "LEFT JOIN invItems ON invItems.itemID = chrInformation.activeCloneID " +
                           "WHERE corporationID=@corporationID AND itemID IN (";

            foreach (PyInteger id in itemIDs)
                parameters["@itemID" + parameters.Count.ToString("X")] = (int) id;

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";

            parameters["@corporationID"] = corporationID;
            
            // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
            // TODO: titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther
            // TODO: divisionID, squadronID
            // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return header.DataFromMySqlReader(0, reader, rowsIndex);
            }
        }

        public PyList<PyTuple> GetOffices(int corporationID, int startPos, int limit, SparseRowsetHeader header, Dictionary<PyDataType, int> rowsIndex)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " itemID, stationID, typeID, officeFolderID " +
                "FROM crpOffices " +
                "WHERE corporationID=@corporationID " +
                "LIMIT @startPos,@limit",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID},
                    {"@startPos", startPos},
                    {"@limit", limit}
                }
            );
            
            using (connection)
            using (reader)
            {
                return header.DataFromMySqlReader(0, reader, rowsIndex);
            }
        }

        public SparseRowsetHeader GetOfficesSparseRowset(int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader =
                Database.PrepareQuery(
                    ref connection,
                    "SELECT COUNT(*) AS recordCount FROM crpOffices WHERE corporationID=@corporationID",
                    new Dictionary<string, object>()
                    {
                        {"@corporationID", corporationID}
                    }
                );
            
            using (connection)
            using (reader)
            {
                PyList<PyString> headers = new PyList<PyString>(4)
                {
                    [0] = "itemID",
                    [1] = "stationID",
                    [2] = "typeID",
                    [3] = "officeFolderID"
                };
                
                FieldType[] fieldTypes = new FieldType[4]
                {
                    FieldType.I4,
                    FieldType.I4,
                    FieldType.I2,
                    FieldType.I4
                };
                
                if (reader.Read() == false)
                    return new SparseRowsetHeader(0, headers, fieldTypes);
                
                return new SparseRowsetHeader(reader.GetInt32(0), headers, fieldTypes);
            }
        }

        public Dictionary<PyDataType, int> GetMembers(int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT characterID FROM chrInformation WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
            
            using (connection)
            using (reader)
            {
                Dictionary<PyDataType, int> result = new Dictionary<PyDataType, int>();
                int index = 0;
                
                while (reader.Read() == true)
                {
                    result[reader.GetInt32(0)] = index ++;
                }

                return result;
            }
        }

        public PyList<PyTuple> GetMembers(PyList<PyInteger> characterIDs, int corporationID, SparseRowsetHeader header, Dictionary<PyDataType, int> rowsIndex)
        {
            // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
            // TODO: titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther
            // TODO: divisionID, squadronID
            // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            
            string query = "SELECT" +
                           " characterID, title, startDateTime, corpRole AS roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
                           " titleMask, 0 AS grantableRoles, 0 AS grantableRolesAtHQ, 0 AS grantableRolesAtBase," +
                           " 0 AS grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, 0 AS baseID, " +
                           " 0 AS blockRoles, gender " +
                           "FROM chrInformation " +
                           "WHERE corporationID=@corporationID AND characterID IN (";

            foreach (PyInteger id in characterIDs)
                parameters["@characterID" + parameters.Count.ToString("X")] = (int) id;

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";

            parameters["@corporationID"] = corporationID;
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return header.DataFromMySqlReader(0, reader, rowsIndex);
            }
        }

        public PyList<PyTuple> GetMembers(int corporationID, int startPos, int limit, SparseRowsetHeader header, Dictionary<PyDataType, int> rowsIndex)
        {
            // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
            // TODO: titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther
            // TODO: divisionID, squadronID
            // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " characterID, title, startDateTime, corpRole AS roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
                " titleMask, 0 AS grantableRoles, 0 AS grantableRolesAtHQ, 0 AS grantableRolesAtBase," + 
                " 0 AS grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, 0 AS baseID, " + 
                " 0 AS blockRoles, gender " +
                "FROM chrInformation " +
                "WHERE corporationID=@corporationID " +
                "LIMIT @startPos,@limit",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID},
                    {"@startPos", startPos},
                    {"@limit", limit}
                }
            );
            
            using (connection)
            using (reader)
            {
                return header.DataFromMySqlReader(0, reader, rowsIndex);
            }
        }

        public SparseRowsetHeader GetMembersSparseRowset(int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader =
                Database.PrepareQuery(
                    ref connection,
                    "SELECT COUNT(*) AS recordCount FROM chrInformation WHERE corporationID=@corporationID",
                    new Dictionary<string, object>()
                    {
                        {"@corporationID", corporationID}
                    }
                );
            
            using (connection)
            using (reader)
            {
                PyList<PyString> headers = new PyList<PyString>(17)
                {
                    [0] = "characterID",
                    [1] = "title",
                    [2] = "startDateTime",
                    [3] = "roles",
                    [4] = "rolesAtHQ",
                    [5] = "rolesAtBase",
                    [6] = "rolesAtOther",
                    [7] = "titleMask",
                    [8] = "grantableRoles",
                    [9] = "grantableRolesAtHQ",
                    [10] = "grantableRolesAtBase",
                    [11] = "gender",
                    [12] = "grantableRolesAtOther",
                    [13] = "divisionID",
                    [14] = "squadronID",
                    [15] = "baseID",
                    [16] = "blockRoles",
                };
                FieldType[] fieldTypes = new FieldType[17]
                {
                    FieldType.I4,
                    FieldType.Str,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I8,
                    FieldType.I1,
                    FieldType.I8,
                    FieldType.I4,
                    FieldType.I4,
                    FieldType.I4,
                    FieldType.I8
                };
                
                if (reader.Read() == false)
                    return new SparseRowsetHeader(0, headers, fieldTypes);

                return new SparseRowsetHeader(reader.GetInt32(0), headers, fieldTypes);
            }
        }

        public Rowset GetRoleGroups()
        {
            return Database.PrepareRowsetQuery("SELECT roleGroupID, roleMask, roleGroupName, isDivisional, appliesTo, appliesToGrantable FROM crpRoleGroups");
        }

        public Rowset GetRoles()
        {
            return Database.PrepareRowsetQuery("SELECT roleID, roleName, shortDescription, description FROM crpRoles");
        }

        public Rowset GetDivisions()
        {
            // TODO: THESE MIGHT BE CUSTOMIZABLE (most likely)
            // TODO: BUT FOR NOW THESE SHOULD BE ENOUGH
            return Database.PrepareRowsetQuery("SELECT divisionID, divisionName, description, leaderType FROM crpNPCDivisions");
        }

        public PyDataType GetTitlesTemplate()
        {
            return Database.PrepareDictRowListQuery(
                "SELECT titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther FROM crpTitlesTemplate"
            );
        }

        public PyDataType GetTitles(int corporationID)
        {
            return Database.PrepareDictRowListQuery(
                "SELECT titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther FROM crpTitles WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public Rowset GetMemberTrackingInfoSimple(int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT characterID, IF(online = 1, -1, IF(lastOnline = 0, NULL, (@currentTicks - lastOnline) / @ticksPerHour)) AS lastOnline FROM chrInformation WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID},
                    {"@ticksPerHour", TimeSpan.TicksPerHour},
                    {"@currentTicks", DateTime.UtcNow.ToFileTimeUtc ()}
                }
            );
        }

        public int GetTitleMaskForCharacter(int characterID, int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT titleMask FROM chrInformation WHERE characterID = @characterID AND corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;

                return reader.GetInt32(0);
            }
        }

        public Dictionary<int, string> GetTitlesNames(int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT titleID, titleName FROM crpTitles WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
            
            using (connection)
            using (reader)
            {
                Dictionary<int, string> result = new Dictionary<int, string>();

                while (reader.Read() == true)
                    result[reader.GetInt32(0)] = reader.GetString(1);

                return result;
            }
        }

        public Rowset GetRecruitmentAdTypes()
        {
            return Database.PrepareRowsetQuery(
                "SELECT typeMask, typeName, description, groupName, dataID, groupDataID FROM crpRecruitmentAdTypes"
            );
        }

        public Rowset GetRecruitmentAds(int? regionID, double? skillPoints, int? typeMask, int? raceMask,
            int? isInAlliance, int? minMembers, int? maxMembers)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string query =
                "SELECT adID, crpRecruitmentAds.corporationID, typeMask, crpRecruitmentAds.description, crpRecruitmentAds.stationID, corporation.allowedMemberRaceIDs AS raceMask, corporation.allianceID FROM crpRecruitmentAds LEFT JOIN corporation ON corporation.corporationID = crpRecruitmentAds.corporationID WHERE 1=1";

            if (regionID is not null)
            {
                // query += " AND "
            }

            if (skillPoints is not null)
            {
                query += " AND minimumSkillPoints >= @skillPoints";
                parameters["@skillPoints"] = skillPoints;
            }

            if (typeMask is not null)
            {
                query += " AND typeMask & @typeMask > 0";
                parameters["@typeMask"] = typeMask;
            }

            if (raceMask is not null)
            {
                query += " AND corporation.allowedMemberRaceIDs & @raceMask > 0";
                parameters["@raceMask"] = raceMask;
            }

            if (isInAlliance is not null)
            {
                if (isInAlliance == 0)
                    query += " AND corporation.allianceID = 0";
                else
                    query += " AND corporation.allianceID > 0";
            }

            if (minMembers is not null)
            {
                query += " AND corporation.memberCount > @minMembers";
                parameters["@minMembers"] = minMembers;
            }

            if (maxMembers is not null)
            {
                query += " AND corporation.memberCount < @maxMembers";
                parameters["@maxMembers"] = maxMembers;
            }

            return Database.PrepareRowsetQuery(query, parameters);
        }

        public Rowset GetMedalsList(int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT medalID, title, description, date, creatorID, noRecepients FROM crpMedals WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public Rowset GetMedalsDetails(int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT crpMedals.medalID, part, graphic, color FROM crpMedalParts LEFT JOIN crpMedals ON crpMedals.medalID = crpMedalParts.medalID WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public Rowset GetCharacterApplications(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT corporationID, characterID, applicationText, roles, grantableRoles, status, applicationDateTime, deleted, lastCorpUpdaterID FROM chrApplications WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetStations(int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT stationID, stationTypeID as typeID FROM staStations WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public PyDataType GetPublicInfo(int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT corporationID, corporationName, allianceID, stationID, ceoID, creatorID, taxRate, memberCount, shares, tickerName, url, description, deleted FROM corporation WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        public int GetCorporationIDForCharacter(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT corporationID FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;

                return reader.GetInt32(0);
            }
        }

        public CorporationDB(DatabaseConnection db) : base(db)
        {
        }
    }
}