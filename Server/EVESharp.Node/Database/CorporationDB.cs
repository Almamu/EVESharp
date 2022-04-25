using System;
using System.Collections.Generic;
using System.Data;
using EVESharp.Common.Database;
using EVESharp.EVE.Alliances;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Corporations;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class CorporationDB : DatabaseAccessor
{
    public const string GET_ALLIANCE_APPLICATIONS   = "CrpGetAllianceApplications";
    public const string LIST_FACTION_CORPORATIONS   = "CrpListFactionCorporations";
    public const string LIST_FACTION_STATION_COUNT  = "CrpListFactionStationCount";
    public const string LIST_FACTION_REGIONS        = "CrpListFactionRegions";
    public const string LIST_FACTION_CONSTELLATIONS = "CrpListFactionConstellations";
    public const string LIST_FACTION_SOLARSYSTEMS   = "CrpListFactionSolarSystems";
    public const string LIST_FACTION_RACES          = "CrpListFactionRaces";
    public const string LIST_NPC_INFO               = "CrpListNPCInfo";
    public const string LIST_NPC_DIVISIONS          = "CrpListNPCDivisions";
    public const string LIST_MEDALS                 = "CrpListMedals";
    public const string LIST_MEDALS_DETAILS         = "CrpListMedalDetails";
    public const string LIST_APPLICATIONS           = "CrpListApplications";
    public const string LIST_SHAREHOLDERS           = "CrpListShareholders";
    public const string GET_ROLE_GROUPS             = "CrpGetRoleGroups";
    public const string GET_ROLES                   = "CrpGetRoles";
    public const string GET_TITLES_TEMPLATE         = "CrpGetTitlesTemplate";
    public const string GET_TITLES                  = "CrpGetTitles";
    public const string GET_RECRUITMENT_AD_TYPES    = "CrpGetRecruitmentAdTypes";

    private ItemDB ItemDB { get; }

    public CorporationDB (ItemDB itemDB, DatabaseConnection db) : base (db)
    {
        ItemDB = itemDB;
    }

    public Rowset GetEveOwners (int corporationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT characterID as ownerID, itemName AS ownerName, typeID FROM chrInformation, eveNames WHERE eveNames.itemID = chrInformation.characterID AND corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public Rowset GetSharesByShareholder (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT corporationID, shares FROM crpShares WHERE ownerID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    public PyDataType GetShareholders (int corporationID)
    {
        return Database.PrepareDictRowListQuery (
            "SELECT ownerID AS shareholderID, crpShares.corporationID, chrInformation.corporationID AS shareholderCorporationID, shares FROM crpShares LEFT JOIN chrInformation ON characterID = ownerID WHERE crpShares.corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public Dictionary <int, int> GetShareholdersList (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT ownerID, shares FROM crpShares WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, int> shares = new Dictionary <int, int> ();

            while (reader.Read ())
                shares [reader.GetInt32 (0)] = reader.GetInt32 (1);

            return shares;
        }
    }

    public int GetSharesForOwner (int corporationID, int ownerID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT shares FROM crpShares WHERE ownerID = @ownerID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@ownerID", ownerID},
                {"@corporationID", corporationID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public void UpdateShares (int corporationID, int ownerID, int newSharesCount)
    {
        Database.PrepareQuery (
            "REPLACE INTO crpShares(ownerID, corporationID, shares) VALUES (@ownerID, @corporationID, @shares)",
            new Dictionary <string, object>
            {
                {"@ownerID", ownerID},
                {"@corporationID", corporationID},
                {"@shares", newSharesCount}
            }
        );
    }

    public Rowset GetMedalsReceived (int characterID, bool publicOnly = false)
    {
        return Database.PrepareRowsetQuery (
            "SELECT medalID, title, description, ownerID, issuerID, chrMedals.date, reason, status FROM chrMedals LEFT JOIN crpMedals USING (medalID) WHERE ownerID = @characterID AND status >= @status",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@status", publicOnly ? 3 : 2}
            }
        );
    }

    public CRowset GetMedalsReceivedDetails (int characterID, bool publicOnly = false)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT medalID, part, graphic, color FROM crpMedalParts LEFT JOIN chrMedals USING(medalID) WHERE ownerID = @characterID AND status >= @status",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@status", publicOnly ? 3 : 2}
            }
        );
    }

    public Rowset GetEmploymentRecord (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT corporationID, startDate, deleted FROM chrEmployment WHERE characterID=@characterID ORDER BY startDate DESC",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    public double GetLPForCharacterCorp (int corporationID, int characterID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT balance FROM chrLPbalance WHERE characterID=@characterID AND corporationID=@corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            // no records means the character doesn't have any LP with the corp yet
            if (reader.Read () == false)
                return 0.0f;

            return reader.GetDouble (0);
        }
    }

    public PyDataType GetMember (int characterID, int corporationID)
    {
        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
        return Database.PrepareKeyValQuery (
            "SELECT" +
            " characterID, title, startDateTime, roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
            " titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase," +
            " grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, baseID, " +
            " blockRoles, gender " +
            "FROM chrInformation " +
            "LEFT JOIN invItems ON invItems.itemID = chrInformation.activeCloneID " +
            "WHERE corporationID=@corporationID AND characterID=@characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID}
            }
        );
    }

    public Dictionary <PyDataType, int> GetOffices (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT officeID FROM crpOffices WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            Dictionary <PyDataType, int> result = new Dictionary <PyDataType, int> ();
            int                          index  = 0;

            while (reader.Read ())
                result [reader.GetInt32 (0)] = index++;

            return result;
        }
    }

    public PyDataType GetOfficesLocation (int corporationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT stationID AS locationID FROM crpOffices WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public Rowset GetAssetsInOfficesAtStation (int corporationID, int stationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT itemID, typeID, locationID, ownerID, flag, contraband, singleton, quantity, groupID, categoryID " +
            "FROM invItems " +
            "LEFT JOIN invTypes USING(typeID) " +
            "LEFT JOIN invGroups USING(groupID) " +
            "WHERE ownerID = @ownerID AND locationID = (SELECT officeID FROM crpOffices WHERE corporationID = @ownerID AND stationID = @stationID) AND flag != @deliveriesFlag",
            new Dictionary <string, object>
            {
                {"@ownerID", corporationID},
                {"@deliveriesFlag", Flags.CorpMarket},
                {"@stationID", stationID}
            }
        );
    }

    public PyList <PyTuple> GetOffices (PyList <PyInteger> itemIDs, int corporationID, SparseRowsetHeader header, Dictionary <PyDataType, int> rowsIndex)
    {
        Dictionary <string, object> parameters = new Dictionary <string, object> ();

        string query = "SELECT" +
                       " officeID, stationID, typeID, officeFolderID " +
                       "FROM crpOffices " +
                       "LEFT JOIN invItems ON itemID = stationID " +
                       "WHERE corporationID=@corporationID AND itemID IN (";

        foreach (PyInteger id in itemIDs)
            parameters ["@itemID" + parameters.Count.ToString ("X")] = (int) id;

        // prepare the correct list of arguments
        query += string.Join (',', parameters.Keys) + ")";

        parameters ["@corporationID"] = corporationID;

        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
        IDbConnection connection = null;
        MySqlDataReader reader     = Database.Select (ref connection, query, parameters);

        using (connection)
        using (reader)
        {
            return header.FetchByKey (0, reader, rowsIndex);
        }
    }

    public PyList <PyTuple> GetOffices (
        PyString columnName, PyList <PyInteger> itemIDs, int corporationID, SparseRowsetHeader header, Dictionary <PyDataType, int> rowsIndex
    )
    {
        if (columnName != "officeID")
            throw new CrpAccessDenied ("INVALID COLUMN");

        Dictionary <string, object> parameters = new Dictionary <string, object> ();

        string query =
            $"SELECT officeID, stationID, typeID, officeFolderID FROM crpOffices LEFT JOIN invItems ON itemID = stationID WHERE crpOffices.corporationID = @corporationID AND {columnName} IN ({PyString.Join (',', itemIDs)}) ";

        parameters ["@corporationID"] = corporationID;

        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
        IDbConnection connection = null;
        MySqlDataReader reader     = Database.Select (ref connection, query, parameters);

        using (connection)
        using (reader)
        {
            return header.FetchByKey (0, reader, rowsIndex);
        }
    }

    public PyList <PyTuple> GetOffices (int corporationID, int startPos, int limit, SparseRowsetHeader header)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT officeID, stationID, typeID, officeFolderID FROM crpOffices LEFT JOIN invItems ON itemID = stationID WHERE corporationID = @corporationID LIMIT @startPos,@limit",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@startPos", startPos},
                {"@limit", limit}
            }
        );

        using (connection)
        using (reader)
        {
            return header.Fetch (0, reader);
        }
    }

    public SparseRowsetHeader GetOfficesSparseRowset (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader =
            Database.Select (
                ref connection,
                "SELECT COUNT(*) AS recordCount FROM crpOffices WHERE corporationID = @corporationID",
                new Dictionary <string, object> {{"@corporationID", corporationID}}
            );

        using (connection)
        using (reader)
        {
            PyList <PyString> headers = new PyList <PyString> (4)
            {
                [0] = "officeID",
                [1] = "stationID",
                [2] = "typeID",
                [3] = "officeFolderID"
            };

            FieldType [] fieldTypes = new FieldType[4] {FieldType.I4, FieldType.I4, FieldType.I2, FieldType.I4};

            if (reader.Read () == false)
                return new SparseRowsetHeader (0, headers, fieldTypes);

            return new SparseRowsetHeader (reader.GetInt32 (0), headers, fieldTypes);
        }
    }

    public Dictionary <PyDataType, int> GetMembers (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT characterID FROM chrInformation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            Dictionary <PyDataType, int> result = new Dictionary <PyDataType, int> ();
            int                          index  = 0;

            while (reader.Read ())
                result [reader.GetInt32 (0)] = index++;

            return result;
        }
    }

    public PyList <PyTuple> GetMembers (PyList <PyInteger> characterIDs, int corporationID, SparseRowsetHeader header, Dictionary <PyDataType, int> rowsIndex)
    {
        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP

        Dictionary <string, object> parameters = new Dictionary <string, object> ();

        string query = "SELECT" +
                       " characterID, title, corporationDateTime AS startDateTime, roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
                       " titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase," +
                       " grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, baseID, " +
                       " blockRoles, gender " +
                       "FROM chrInformation " +
                       "WHERE corporationID=@corporationID AND characterID IN (";

        foreach (PyInteger id in characterIDs)
            parameters ["@characterID" + parameters.Count.ToString ("X")] = (int) id;

        // prepare the correct list of arguments
        query += string.Join (',', parameters.Keys) + ")";

        parameters ["@corporationID"] = corporationID;
        IDbConnection connection = null;
        MySqlDataReader reader     = Database.Select (ref connection, query, parameters);

        using (connection)
        using (reader)
        {
            return header.FetchByKey (0, reader, rowsIndex);
        }
    }

    public PyList <PyTuple> GetMembers (int corporationID, int startPos, int limit, SparseRowsetHeader header, Dictionary <PyDataType, int> rowsIndex)
    {
        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT" +
            " characterID, title, corporationDateTime AS startDateTime, roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
            " titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase," +
            " grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, baseID, " +
            " blockRoles, gender " +
            "FROM chrInformation " +
            "WHERE corporationID=@corporationID " +
            "LIMIT @startPos,@limit",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@startPos", startPos},
                {"@limit", limit}
            }
        );

        using (connection)
        using (reader)
        {
            return header.FetchByKey (0, reader, rowsIndex);
        }
    }

    public PyList <PyTuple> GetMembers (int corporationID, int startPos, int limit, SparseRowsetHeader header)
    {
        // TODO: GENERATE PROPER FIELDS FOR THE FOLLOWING FIELDS
        // TODO: divisionID, squadronID
        // TODO: CHECK IF THIS startDateTime IS THE CORP'S MEMBERSHIP OR CHARACTER'S MEMBERSHIP
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT" +
            " characterID, title, corporationDateTime AS startDateTime, roles, rolesAtHQ, rolesAtBase, rolesAtOther," +
            " titleMask, grantableRoles, grantableRolesAtHQ, grantableRolesAtBase," +
            " grantableRolesAtOther, 0 AS divisionID, 0 AS squadronID, baseID, " +
            " blockRoles, gender " +
            "FROM chrInformation " +
            "WHERE corporationID=@corporationID " +
            "LIMIT @startPos,@limit",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@startPos", startPos},
                {"@limit", limit}
            }
        );

        using (connection)
        using (reader)
        {
            return header.Fetch (0, reader);
        }
    }

    public SparseRowsetHeader GetMembersSparseRowset (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader =
            Database.Select (
                ref connection,
                "SELECT COUNT(*) AS recordCount FROM chrInformation WHERE corporationID=@corporationID",
                new Dictionary <string, object> {{"@corporationID", corporationID}}
            );

        using (connection)
        using (reader)
        {
            PyList <PyString> headers = new PyList <PyString> (17)
            {
                [0]  = "characterID",
                [1]  = "title",
                [2]  = "startDateTime",
                [3]  = "roles",
                [4]  = "rolesAtHQ",
                [5]  = "rolesAtBase",
                [6]  = "rolesAtOther",
                [7]  = "titleMask",
                [8]  = "grantableRoles",
                [9]  = "grantableRolesAtHQ",
                [10] = "grantableRolesAtBase",
                [11] = "grantableRolesAtOther",
                [12] = "divisionID",
                [13] = "squadronID",
                [14] = "baseID",
                [15] = "blockRoles",
                [16] = "gender"
            };
            FieldType [] fieldTypes = new FieldType[17]
            {
                FieldType.I4,
                FieldType.Str,
                FieldType.I8,
                FieldType.I8,
                FieldType.I8,
                FieldType.I8,
                FieldType.I8,
                FieldType.I4,
                FieldType.I8,
                FieldType.I8,
                FieldType.I8,
                FieldType.I8,
                FieldType.I4,
                FieldType.I4,
                FieldType.I4,
                FieldType.I4,
                FieldType.Bool
            };

            if (reader.Read () == false)
                return new SparseRowsetHeader (0, headers, fieldTypes);

            return new SparseRowsetHeader (reader.GetInt32 (0), headers, fieldTypes);
        }
    }

    public Rowset GetMemberTrackingInfoSimple (int corporationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT characterID, title, baseID, corporationDateTime AS startDateTime, corporationID, IF(online = 1, -1, IF(lastOnline = 0, NULL, (@currentTicks - lastOnline) / @ticksPerHour)) AS lastOnline, logonDateTime, logoffDateTime FROM chrInformation WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@ticksPerHour", TimeSpan.TicksPerHour},
                {"@currentTicks", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public Rowset GetMemberTrackingInfo (int corporationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT characterID, shp.typeID AS shipTypeID, shp.locationID AS locationID, baseID, corporationDateTime AS startDateTime, title, corporationID, logonDateTime, logoffDateTime, roles, grantableRoles, IF(online = 1, -1, IF(lastOnline = 0, NULL, (@currentTicks - lastOnline) / @ticksPerHour)) AS lastOnline FROM chrInformation LEFT JOIN invItems chr ON chr.itemID = characterID LEFT JOIN invItems shp ON shp.itemID = chr.locationID WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@ticksPerHour", TimeSpan.TicksPerHour},
                {"@currentTicks", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public Rowset GetMemberTrackingInfo (int corporationID, int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT characterID, shp.typeID AS shipTypeID, shp.locationID AS locationID, baseID, corporationDateTime AS startDateTime, title, corporationID, logonDateTime, logoffDateTime, roles, grantableRoles, IF(online = 1, -1, IF(lastOnline = 0, NULL, (@currentTicks - lastOnline) / @ticksPerHour)) AS lastOnline FROM chrInformation LEFT JOIN invItems chr ON chr.itemID = characterID LEFT JOIN invItems shp ON shp.itemID = chr.locationID WHERE corporationID = @corporationID AND characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@ticksPerHour", TimeSpan.TicksPerHour},
                {"@currentTicks", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public int GetTitleMaskForCharacter (int characterID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT titleMask FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public void GetCorporationInformationForCharacter (int characterID, out string title, out int titleMask, out int corporationID, out int? allianceID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT title, titleMask, corporationID, allianceID FROM chrInformation LEFT JOIN corporation USING(corporationID) WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        title         = "";
        titleMask     = 0;
        corporationID = 0;
        allianceID    = 0;

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return;

            title         = reader.GetString (0);
            titleMask     = reader.GetInt32 (1);
            corporationID = reader.GetInt32 (2);
            allianceID    = reader.GetInt32OrNull (3);
        }
    }

    public Dictionary <int, string> GetTitlesNames (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT titleID, titleName FROM crpTitles WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, string> result = new Dictionary <int, string> ();

            while (reader.Read ())
                result [reader.GetInt32 (0)] = reader.GetString (1);

            return result;
        }
    }

    public Rowset GetRecruitmentAds (
        int? regionID,     double? skillPoints, int? typeMask,   int? raceMask,
        int? isInAlliance, int?    minMembers,  int? maxMembers, int? corporationID = null
    )
    {
        Dictionary <string, object> parameters = new Dictionary <string, object> ();
        string query =
            "SELECT adID, crpRecruitmentAds.corporationID, 24 AS channelID, typeMask, crpRecruitmentAds.description, crpRecruitmentAds.stationID, raceMask, corporation.allianceID, expiryDateTime, createDateTime, regionID, constellationID, solarSystemID, minimumSkillPoints AS skillPoints FROM crpRecruitmentAds LEFT JOIN corporation ON crpRecruitmentAds.corporationID = corporation.corporationID LEFT JOIN staStations ON crpRecruitmentAds.stationID = staStations.stationID WHERE 1=1";

        if (regionID is not null)
        {
            query                    += " AND regionID = @regionID";
            parameters ["@regionID"] =  regionID;
        }

        if (skillPoints is not null)
        {
            query                       += " AND minimumSkillPoints <= @skillPoints";
            parameters ["@skillPoints"] =  skillPoints;
        }

        if (typeMask is not null)
        {
            query                    += " AND typeMask & @typeMask > 0";
            parameters ["@typeMask"] =  typeMask;
        }

        if (raceMask is not null)
        {
            query                    += " AND corporation.allowedMemberRaceIDs & @raceMask > 0";
            parameters ["@raceMask"] =  raceMask;
        }

        if (isInAlliance is not null)
        {
            if (isInAlliance == 0)
                query += " AND corporation.allianceID IS NULL";
            else
                query += " AND corporation.allianceID IS NOT NULL";
        }

        if (minMembers is not null)
        {
            query                      += " AND corporation.memberCount >= @minMembers";
            parameters ["@minMembers"] =  minMembers;
        }

        if (maxMembers is not null)
        {
            query                      += " AND corporation.memberCount <= @maxMembers";
            parameters ["@maxMembers"] =  maxMembers;
        }

        if (corporationID is not null)
        {
            query                         += " AND corporation.corporationID = @corporationID";
            parameters ["@corporationID"] =  corporationID;
        }

        return Database.PrepareRowsetQuery (query, parameters);
    }

    public Rowset GetCharacterApplications (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT corporationID, characterID, applicationText, 0 AS status, applicationDateTime FROM chrApplications WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    public Rowset GetStations (int corporationID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT stationID, stationTypeID as typeID FROM staStations WHERE corporationID = @corporationID UNION SELECT stationID, stationTypeID AS typeID FROM crpOffices LEFT JOIN staStations USING(stationID) WHERE crpOffices.corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public PyDataType GetPublicInfo (int corporationID)
    {
        return Database.PrepareKeyValQuery (
            "SELECT corporationID, corporationName, allianceID, stationID, ceoID, creatorID, taxRate, memberCount, shares, tickerName, url, description, deleted FROM corporation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public int GetCorporationIDForCharacter (int characterID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT corporationID FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public int CreateCorporation (
        string name,      string description, string ticker,      string url,    double taxRate,
        int    creatorID, int    stationID,   int    memberLimit, int    raceID, int    allowedMemberRaceIDs, int? shape1, int? shape2,
        int?   shape3,
        int?   color1, int? color2, int? color3, string typeface
    )
    {
        // create the item first
        int corporationID = (int) ItemDB.CreateItem (
            name, (int) Types.Corporation, creatorID, stationID, Flags.None, false,
            true, 1, 0, 0, 0, ""
        );

        Database.PrepareQuery (
            "INSERT INTO corporation(" +
            "corporationID, corporationName, description, tickerName, url, taxRate, creatorID, ceoID, stationID, memberCount, memberLimit, raceID, allowedMemberRaceIDs, shape1, shape2, shape3, color1, color2, color3, typeface" +
            ")VALUES(" +
            "@corporationID, @corporationName, @description, @tickerName, @url, @taxRate, @creatorID, @creatorID, @stationID, 1, @memberLimit, @raceID, @allowedMemberRaceIDs, @shape1, @shape2, @shape3, @color1, @color2, @color3, @typeface" +
            ")",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@corporationName", name},
                {"@description", description},
                {"@tickerName", ticker},
                {"@url", url},
                {"@taxRate", taxRate},
                {"@creatorID", creatorID},
                {"@stationID", stationID},
                {"@memberLimit", memberLimit},
                {"@raceID", raceID},
                {"@allowedMemberRaceIDs", allowedMemberRaceIDs},
                {"@shape1", shape1},
                {"@shape2", shape2},
                {"@shape3", shape3},
                {"@color1", color1},
                {"@color2", color2},
                {"@color3", color3},
                {"@typeface", typeface}
            }
        );

        return corporationID;
    }

    public void CreateDefaultTitlesForCorporation (int corporationID)
    {
        Database.PrepareQuery (
            "INSERT INTO crpTitles SELECT @corporationID AS corporationID, titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther FROM crpTitlesTemplate",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public bool IsCorporationNameTaken (string corporationName)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT COUNT(*) FROM eveNames WHERE groupID = {(int) Groups.Corporation} AND itemName LIKE @corporationName",
            new Dictionary <string, object> {{"@corporationName", corporationName}}
        );

        using (connection)
        using (reader)
        {
            reader.Read ();

            return reader.GetInt32 (0) > 0;
        }
    }

    public bool IsTickerNameTaken (string tickerName)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT COUNT(*) FROM corporation WHERE tickerName LIKE @tickerName",
            new Dictionary <string, object> {{"@tickerName", tickerName}}
        );

        using (connection)
        using (reader)
        {
            reader.Read ();

            return reader.GetInt32 (0) > 0;
        }
    }

    public bool IsAllianceNameTaken (string corporationName)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            $"SELECT COUNT(*) FROM eveNames WHERE groupID = {(int) Groups.Alliance} AND itemName LIKE @corporationName",
            new Dictionary <string, object> {{"@corporationName", corporationName}}
        );

        using (connection)
        using (reader)
        {
            reader.Read ();

            return reader.GetInt32 (0) > 0;
        }
    }

    public bool IsShortNameTaken (string shortName)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT COUNT(*) FROM crpAlliances WHERE shortName LIKE @shortName",
            new Dictionary <string, object> {{"@shortName", shortName}}
        );

        using (connection)
        using (reader)
        {
            reader.Read ();

            return reader.GetInt32 (0) > 0;
        }
    }

    public void UpdateDivisions (
        int    corporationID, string division1, string division2, string division3,
        string division4,     string division5, string division6, string division7, string wallet1, string wallet2,
        string wallet3,       string wallet4,   string wallet5,   string wallet6,   string wallet7
    )
    {
        Database.PrepareQuery (
            "UPDATE corporation SET division1 = @division1, division2 = @division2, division3 = @division3, division4 = @division4, division5 = @division5, division6 = @division6, division7 = @division7, walletDivision1 = @walletDivision1, walletDivision2 = @walletDivision2, walletDivision3 = @walletDivision3, walletDivision4 = @walletDivision4, walletDivision5 = @walletDivision5, walletDivision6 = @walletDivision6, walletDivision7 = @walletDivision7 WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@division1", division1},
                {"@division2", division2},
                {"@division3", division3},
                {"@division4", division4},
                {"@division5", division5},
                {"@division6", division6},
                {"@division7", division7},
                {"@walletDivision1", wallet1},
                {"@walletDivision2", wallet2},
                {"@walletDivision3", wallet3},
                {"@walletDivision4", wallet4},
                {"@walletDivision5", wallet5},
                {"@walletDivision6", wallet6},
                {"@walletDivision7", wallet7},
                {"@corporationID", corporationID}
            }
        );
    }

    public void UpdateCorporation (int corporationID, string description, string url, double tax)
    {
        Database.PrepareQuery (
            "UPDATE corporation SET description = @description, taxRate = @taxRate, url = @url WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@description", description},
                {"@url", url},
                {"@taxRate", tax}
            }
        );
    }

    public IEnumerable <int> GetMembersForCorp (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT characterID FROM chrInformation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                yield return reader.GetInt32 (0);
        }
    }

    public CRowset GetItemsRented (int corporationID)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT crpOffices.typeID, stationID AS rentedFromID, invItems.typeID AS stationTypeID, startDate, rentPeriodInDays, periodCost, balanceDueDate FROM crpOffices LEFT JOIN invItems ON invItems.itemID = crpOffices.stationID WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public ulong CreateRecruitmentAd (int stationID, int days, int corporationID, int typeMask, int raceMask, string description, int skillPoints)
    {
        return Database.PrepareQueryLID (
            "INSERT INTO crpRecruitmentAds(expiryDateTime, createDateTime, corporationID, typeMask, raceMask, description, minimumSkillPoints, stationID)VALUES(@expiryDateTime, @createDateTime, @corporationID, @typeMask, @raceMask, @description, @minimumSkillpoints, @stationID)",
            new Dictionary <string, object>
            {
                {"@expiryDateTime", DateTime.UtcNow.AddDays (days).ToFileTimeUtc ()},
                {"@createDateTime", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@corporationID", corporationID},
                {"@typeMask", typeMask},
                {"@raceMask", raceMask},
                {"@description", description},
                {"@minimumSkillpoints", skillPoints},
                {"@stationID", stationID}
            }
        );
    }

    public bool DeleteRecruitmentAd (int advertId, int corporationID)
    {
        return Database.PrepareQuery (
            "DELETE FROM crpRecruitmentAds WHERE adID = @adID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@adID", advertId},
                {"@corporationID", corporationID}
            }
        ) > 0;
    }

    public bool UpdateRecruitmentAd (int adID, int corporationID, int typeMask, int raceMask, string description, int skillPoints)
    {
        return Database.PrepareQuery (
            "UPDATE crpRecruitmentAds SET typeMask = @typeMask, raceMask = @raceMask, description = @description, minimumSkillPoints = @minimumSkillpoints WHERE adID = @adID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@adID", adID},
                {"@corporationID", corporationID},
                {"@typeMask", typeMask},
                {"@raceMask", raceMask},
                {"@description", description},
                {"@minimumSkillpoints", skillPoints}
            }
        ) > 0;
    }

    public PyDataType GetCorporationRow (int corporationID)
    {
        return Database.PrepareRowQuery (
            "SELECT corporationID, corporationName, description, tickerName, url, taxRate, minimumJoinStanding, corporationType, hasPlayerPersonnelManager, sendCharTerminationMessage, creatorID, ceoID, stationID, raceID, allianceID, shares, memberCount, memberLimit, allowedMemberRaceIDs, graphicID, shape1, shape2, shape3, color1, color2, color3, typeface, division1, division2, division3, division4, division5, division6, division7, walletDivision1, walletDivision2, walletDivision3, walletDivision4, walletDivision5, walletDivision6, walletDivision7, deleted FROM corporation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public void UpdateTitle (
        int  corporationID, int  titleID,            string titleName,   long roles,                long grantableRoles,
        long rolesAtHQ,     long grantableRolesAtHQ, long   rolesAtBase, long grantableRolesAtBase, long rolesAtOther,
        long grantableRolesAtOther
    )
    {
        Database.PrepareQuery (
            "REPLACE INTO crpTitles(corporationID, titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther)VALUES(@corporationID, @titleID, @titleName, @roles, @grantableRoles, @rolesAtHQ, @grantableRolesAtHQ, @rolesAtBase, @grantableRolesAtBase, @rolesAtOther, @grantableRolesAtOther)",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@titleID", titleID},
                {"@titleName", titleName},
                {"@roles", roles},
                {"@grantableRoles", grantableRoles},
                {"@rolesAtHQ", rolesAtHQ},
                {"@grantableRolesAtHQ", grantableRolesAtHQ},
                {"@rolesAtBase", rolesAtBase},
                {"@grantableRolesAtBase", grantableRolesAtBase},
                {"@rolesAtOther", rolesAtOther},
                {"@grantableRolesAtOther", grantableRolesAtOther}
            }
        );
    }

    /// <summary>
    /// Gets the effective roles for the given titleMask
    /// </summary>
    /// <param name="corporationID">The corporation to fetch the roles from</param>
    /// <param name="titleMask">The list of titles to get the roles from</param>
    /// <param name="roles"></param>
    /// <param name="rolesAtHQ"></param>
    /// <param name="rolesAtBase"></param>
    /// <param name="rolesAtOther"></param>
    /// <param name="grantableRoles"></param>
    /// <param name="grantableRolesAtHQ"></param>
    /// <param name="grantableRolesAtBase"></param>
    /// <param name="grantableRolesAtOther"></param>
    /// <param name="titleName"></param>
    public void GetTitleInformation (
        int      corporationID,      long titleMask, out long roles, out long rolesAtHQ, out long rolesAtBase, out long rolesAtOther, out long grantableRoles,
        out long grantableRolesAtHQ, out long grantableRolesAtBase, out long grantableRolesAtOther, out string titleName
    )
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, titleName FROM crpTitles WHERE corporationID = @corporationID AND titleID & @titleMask > 0",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@titleMask", titleMask}
            }
        );

        using (connection)
        using (reader)
        {
            // set roles to a default, 0 value first
            roles                 = 0;
            rolesAtHQ             = 0;
            rolesAtBase           = 0;
            rolesAtOther          = 0;
            grantableRoles        = 0;
            grantableRolesAtBase  = 0;
            grantableRolesAtOther = 0;
            grantableRolesAtHQ    = 0;
            titleName             = "";

            while (reader.Read ())
            {
                roles                 |= reader.GetInt64 (0);
                grantableRoles        |= reader.GetInt64 (1);
                rolesAtHQ             |= reader.GetInt64 (2);
                grantableRolesAtHQ    |= reader.GetInt64 (3);
                rolesAtBase           |= reader.GetInt64 (4);
                grantableRolesAtBase  |= reader.GetInt64 (5);
                rolesAtOther          |= reader.GetInt64 (6);
                grantableRolesAtOther |= reader.GetInt64 (7);
                titleName             =  reader.GetString (8);
            }
        }
    }

    public List <CorporationOffice> FindOfficesCloseToRenewal ()
    {
        long expirationDate = DateTime.Now.AddDays (30).ToFileTimeUtc ();

        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT corporationID, officeID, periodCost, stationID, balanceDueDate FROM crpOffices WHERE nextBillID IS NULL AND balanceDueDate < @expirationDate",
            new Dictionary <string, object> {{"@expirationDate", expirationDate}}
        );

        using (connection)
        using (reader)
        {
            List <CorporationOffice> result = new List <CorporationOffice> ();

            while (reader.Read ())
                result.Add (
                    new CorporationOffice
                    {
                        CorporationID = reader.GetInt32 (0),
                        OfficeID      = reader.GetInt32 (1),
                        PeriodCost    = reader.GetInt32 (2),
                        StationID     = reader.GetInt32 (3),
                        DueDate       = reader.GetInt64 (4)
                    }
                );

            return result;
        }
    }

    public void SetNextBillID (int corporationID, int officeID, int newBillID)
    {
        Database.PrepareQuery (
            "UPDATE crpOffices SET nextBillID = @nextBillID, balanceDueDate = balanceDueDate + @interval WHERE officeID = @officeID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@nextBillID", newBillID},
                {"@interval", TimeSpan.FromDays (30).Ticks},
                {"@officeID", officeID},
                {"@corporationID", corporationID}
            }
        );
    }

    public void UpdateMemberLimits (int corporationID, int newMemberLimit, int newRaceMask)
    {
        Database.PrepareQuery (
            "UPDATE corporation SET memberLimit = @memberLimit, allowedMemberRaceIDs = @allowedMemberRaceIDs WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@memberLimit", newMemberLimit},
                {"@allowedMemberRaceIDs", newRaceMask},
                {"@corporationID", corporationID}
            }
        );
    }

    public void CreateApplication (int characterID, int corporationID, string message)
    {
        Database.PrepareQuery (
            "INSERT INTO chrApplications(characterID, corporationID, applicationText, applicationDateTime)VALUES(@characterID, @corporationID, @message, @datetime)",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@message", message},
                {"@datetime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public void DeleteApplication (int characterID, int corporationID)
    {
        Database.PrepareQuery (
            "DELETE FROM chrApplications WHERE characterID = @characterID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID}
            }
        );
    }

    public void CreateMedal (int corporationID, int creatorID, string title, string description, PyList <PyList> parts)
    {
        ulong medalID = Database.PrepareQueryLID (
            "INSERT INTO crpMedals(corporationID, title, description, date, creatorID, noRecepients)VALUES(@corporationID, @title, @description, @date, @creatorID, @noRecepients)",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@title", title},
                {"@description", description},
                {"@date", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@creatorID", creatorID},
                {"@noRecepients", 0}
            }
        );

        int index = 0;

        IDbConnection connection = null;
        MySqlCommand command = Database.PrepareQuery (
            ref connection,
            "INSERT INTO crpMedalParts(medalID, `index`, part, graphic, color)VALUE(@medalID, @index, @part, @graphic, @color)"
        );

        using (connection)
        using (command)
        {
            foreach (PyList entry in parts)
            {
                command.Parameters.Clear ();

                command.Parameters.AddWithValue ("@medalID", medalID);
                command.Parameters.AddWithValue ("@index",   index++);
                command.Parameters.AddWithValue ("@part",    entry [0] as PyInteger);
                command.Parameters.AddWithValue ("@graphic", entry [1] as PyString);
                command.Parameters.AddWithValue ("@color",   entry [2] as PyInteger);

                command.ExecuteNonQuery ();
            }
        }
    }

    public void GrantMedal (int medalID, int ownerID, int issuerID, string reason, int status = 0)
    {
        Database.PrepareQuery (
            "INSERT INTO chrMedals(medalID, ownerID, issuerID, date, reason, status)VALUE(@medalID, @ownerID, @issuerID, @date, @reason, @status)",
            new Dictionary <string, object>
            {
                {"@medalID", medalID},
                {"@ownerID", ownerID},
                {"@issuerID", issuerID},
                {"@date", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@reason", reason},
                {"@status", status}
            }
        );
    }

    public PyDataType GetRecipientsOfMedal (int medalID)
    {
        return Database.PreparePackedRowListQuery (
            "SELECT ownerID AS recepientID, issuerID, date, reason, status FROM chrMedals WHERE medalID = @medalID",
            new Dictionary <string, object> {{"@medalID", medalID}}
        );
    }

    public void RemoveMedalFromCharacter (int medalID, int characterID)
    {
        Database.PrepareQuery (
            "DELETE FROM chrMedals WHERE ownerID = @characterID AND medalID = @medalID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@medalID", medalID}
            }
        );
    }

    public void UpdateMedalForCharacter (int medalID, int characterID, int newStatus)
    {
        Database.PrepareQuery (
            "UPDATE chrMedals SET status = @status WHERE ownerID = @characterID AND medalID = @medalID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@medalID", medalID},
                {"@status", newStatus}
            }
        );
    }

    public PyDataType GetMedalDetails (int medalID)
    {
        return Database.PrepareKeyValQuery (
            "SELECT title, description, noRecepients AS numberOfRecipients, corporationID AS ownerID FROM crpMedals WHERE medalID = @medalID",
            new Dictionary <string, object> {{"@medalID", medalID}}
        );
    }

    public void IncreaseRecepientsForMedal (int medalID, int amount)
    {
        Database.PrepareQuery (
            "UPDATE crpMedals SET noRecepients = noRecepients + @amount WHERE medalID = @medalID",
            new Dictionary <string, object>
            {
                {"@medalID", medalID},
                {"@amount", amount}
            }
        );
    }

    public void RemoveExpiredCorporationAds ()
    {
        Database.PrepareQuery (
            "DELETE FROM crpRecruitmentAds WHERE expiryDateTime < @currentTime",
            new Dictionary <string, object> {{"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}}
        );
    }

    public PyDataType GetMemberIDsWithMoreThanAvgShares (int corporationID)
    {
        return Database.PrepareList (
            "SELECT ownerID FROM crpshares LEFT JOIN chrinformation ON ownerID = characterID WHERE shares > (SELECT AVG(shares) FROM crpshares LEFT JOIN chrinformation ON ownerID = characterID WHERE crpShares.corporationID = @corporationID AND chrInformation.corporationID = @corporationID) AND crpShares.corporationID = @corporationID AND chrInformation.corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public ulong InsertVoteCase (int corporationID, int characterID, int type, long startDateTime, long endDateTime, string text, string description)
    {
        return Database.PrepareQueryLID (
            "INSERT INTO crpVotes(voteType, corporationID, characterID, startDateTime, endDateTime, voteCaseText, description)VALUE(@type, @corporationID, @characterID, @startDateTime, @endDateTime, @text, @description)",
            new Dictionary <string, object>
            {
                {"@type", type},
                {"@corporationID", corporationID},
                {"@characterID", characterID},
                {"@startDateTime", startDateTime},
                {"@endDateTime", endDateTime},
                {"@text", text},
                {"@description", description}
            }
        );
    }

    public void InsertVoteOption (int voteCaseID, string text, int parameter, int? parameter1, int? parameter2)
    {
        Database.PrepareQuery (
            "INSERT INTO crpVoteOptions(voteCaseID, optionText, parameter, parameter1, parameter2)VALUE(@voteCaseID, @text, @parameter, @parameter1, @parameter2)",
            new Dictionary <string, object>
            {
                {"@voteCaseID", voteCaseID},
                {"@text", text},
                {"@parameter", parameter},
                {"@parameter1", parameter1},
                {"@parameter2", parameter2}
            }
        );
    }

    public PyDataType GetOpenVoteCasesByCorporation (int corporationID)
    {
        return Database.PrepareIndexRowsetQuery (
            0,
            "SELECT voteCaseID, voteType, corporationID, characterID, startDateTime, endDateTime, voteCaseText, description FROM crpVotes WHERE corporationID = @corporationID AND endDateTime > @currentTime",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType GetAllVoteCasesByCorporation (int corporationID)
    {
        return Database.PrepareIndexRowsetQuery (
            0,
            "SELECT voteCaseID, voteType, corporationID, characterID, startDateTime, endDateTime, voteCaseText, description FROM crpVotes WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );
    }

    public PyDataType GetSanctionedActionsByCorporation (int corporationID, int status)
    {
        return Database.PrepareRowsetQuery (
            "SELECT voteCaseID, voteType, corporationID, chrVotes.characterID, startDateTime, endDateTime, voteCaseText, description, COUNT(*) AS votes, parameter, parameter1, parameter2, 0 AS actedUpon, 0 AS inEffect, @time AS expires, NULL AS timeRescended, NULL AS timeActedUpon FROM crpvotes RIGHT JOIN crpvoteoptions USING(voteCaseID) RIGHT JOIN chrVotes USING(optionID) WHERE corporationID = @corporationID AND status = @status GROUP BY optionID ORDER BY votes DESC",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@status", status},
                {"@time", DateTime.UtcNow.AddDays (20).ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType GetClosedVoteCasesByCorporation (int corporationID)
    {
        return Database.PrepareIndexRowsetQuery (
            0,
            "SELECT voteCaseID, voteType, corporationID, characterID, startDateTime, endDateTime, voteCaseText, description FROM crpVotes WHERE corporationID = @corporationID AND endDateTime < @currentTime",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType GetVoteCaseOptions (int corporationID, int voteCaseID)
    {
        return Database.PrepareIndexRowsetQuery (
            0,
            "SELECT optionID, optionText, parameter, parameter1, parameter2, IF(crpVotes.endDateTime < @currentTime, (SELECT COUNT(*) FROM chrVotes WHERE optionID = crpVoteOptions.optionID), 0) AS votesFor FROM crpVoteOptions LEFT JOIN crpVotes USING(voteCaseID) WHERE voteCaseID = @voteCaseID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@voteCaseID", voteCaseID},
                {"@corporationID", corporationID},
                {"@currentTime", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public PyDataType GetVotes (int corporationID, int voteCaseID, int characterID)
    {
        return Database.PrepareIndexRowsetQuery (
            0,
            "SELECT chrVotes.characterID, optionID FROM chrVotes LEFT JOIN crpVoteOptions USING(optionID) LEFT JOIN crpVotes USING(voteCaseID) WHERE voteCaseID = @voteCaseID AND corporationID = @corporationID AND chrVotes.characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@voteCaseID", voteCaseID},
                {"@corporationID", corporationID},
                {"@characterID", characterID}
            }
        );
    }

    public void InsertVote (int voteCaseID, int optionID, int characterID)
    {
        Database.PrepareQuery (
            "INSERT IGNORE INTO chrVotes(optionID, characterID)VALUE(@optionID, @characterID)",
            new Dictionary <string, object>
            {
                {"@optionID", optionID},
                {"@characterID", characterID}
            }
        );
    }

    public void UpdateCorporationInformation (Corporation corporation)
    {
        this.UpdateCorporationInformation (corporation.ID, corporation.AllianceID, corporation.StartDate, corporation.ExecutorCorpID);
    }

    public void UpdateCorporationInformation (int corporationID, int? allianceID, long? startDate, int? executorCorpID)
    {
        Database.PrepareQuery (
            "UPDATE corporation SET allianceID = @allianceID, startDate = @startDate, chosenExecutorID = @chosenExecutorID WHERE corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@allianceID", allianceID},
                {"@startDate", startDate},
                {"@chosenExecutorID", executorCorpID}
            }
        );
    }

    public int? GetAllianceIDForCorporation (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT allianceID FROM corporation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return reader.GetInt32OrNull (0);
        }
    }

    public int? GetCurrentAllianceApplication (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT allianceID FROM crpApplications WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return reader.GetInt32 (0);
        }
    }

    public void InsertAllianceApplication (int allianceID, int corporationID, string text)
    {
        Database.PrepareQuery (
            "REPLACE INTO crpApplications(allianceID, corporationID, applicationText, applicationDateTime, applicationUpdateTime, state)VALUES(@allianceID, @corporationID, @applicationText, @applicationDateTime, @applicationDateTime, @state)",
            new Dictionary <string, object>
            {
                {"@allianceID", allianceID},
                {"@corporationID", corporationID},
                {"@applicationText", text},
                {"@applicationDateTime", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@state", (int) ApplicationStatus.New}
            }
        );
    }

    public long GetAllianceJoinDate (int corporationID)
    {
        IDbConnection connection = null;
        MySqlDataReader reader = Database.Select (
            ref connection,
            "SELECT startDate FROM corporation WHERE corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt64 (0);
        }
    }
}