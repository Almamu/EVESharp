/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types.Information;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Data.Inventory.Exceptions;
using EVESharp.Node.Dogma;
using EVESharp.PythonTypes.Types.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class ItemDB : DatabaseAccessor
{
    private IAttributes  AttributeManager { get; }
    private ITypes Types      { get; }
    private IStations    StationManager   { get; }

    public ItemDB (IDatabaseConnection db, IAttributes attributes, ITypes types, IStations stations) : base (db)
    {
        this.AttributeManager = attributes;
        this.Types      = types;
        this.StationManager   = stations;
    }

    public IEnumerable <Item> LoadStaticItems ()
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            $"SELECT itemID, eveNames.itemName, invItems.typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, custominfo FROM invItems LEFT JOIN eveNames USING(itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID < {ItemRanges.USERGENERATED_ID_MIN} AND (groupID = {(int) EVE.Data.Inventory.GroupID.Station} OR groupID = {(int) EVE.Data.Inventory.GroupID.Faction} OR groupID = {(int) EVE.Data.Inventory.GroupID.SolarSystem} OR groupID = {(int) EVE.Data.Inventory.GroupID.Corporation} OR groupID = {(int) EVE.Data.Inventory.GroupID.System})"
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                yield return this.BuildItemFromReader (reader);
        }
    }

    private Item BuildItemFromReader (DbDataReader reader)
    {
        Type itemType = this.Types [reader.GetInt32 (2)];

        return new Item
        {
            ID         = reader.GetInt32 (0),
            Name       = reader.GetStringOrNull (1),
            Type       = this.Types [reader.GetInt32 (2)],
            OwnerID    = reader.GetInt32 (3),
            LocationID = reader.GetInt32 (4),
            Flag       = (Flags) reader.GetInt32 (5),
            Contraband = reader.GetBoolean (6),
            Singleton  = reader.GetBoolean (7),
            Quantity   = reader.GetInt32 (8),
            X          = reader.GetDoubleOrNull (9),
            Y          = reader.GetDoubleOrNull (10),
            Z          = reader.GetDoubleOrNull (11),
            CustomInfo = reader.GetStringOrNull (12),
            Attributes = new AttributeList (itemType, this.LoadAttributesForItem (reader.GetInt32 (0)))
        };
    }

    public Item LoadItem (int itemID, long nodeID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID, eveNames.itemName, invItems.typeID, ownerID, locationID, flag, contraband, singleton, quantity, x, y, z, customInfo, nodeID FROM invItems LEFT JOIN eveNames USING (itemID) LEFT JOIN invPositions USING (itemID) WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            Item newItem = this.BuildItemFromReader (reader);

            // the non-user generated items cannot be owned by any node
            if (itemID < ItemRanges.USERGENERATED_ID_MIN)
                return newItem;

            if (reader.IsDBNull (13) == false && reader.GetInt32 (13) != 0)
                throw new ItemNotLoadedException (itemID, "Trying to load an item that is loaded on another node!");

            // Update the database information
            Database.InvSetItemNode (itemID, nodeID);

            return newItem;
        }
    }

    public List <int> GetInventoryItems (int inventoryID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID FROM invItems WHERE locationID = @inventoryID",
            new Dictionary <string, object> {{"@inventoryID", inventoryID}}
        );

        using (connection)
        using (reader)
        {
            List <int> itemList = new List <int> ();

            while (reader.Read ()) itemList.Add (reader.GetInt32 (0));

            return itemList;
        }
    }

    public Blueprint LoadBlueprint (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT copy, materialLevel, productivityLevel, licensedProductionRunsRemaining FROM invBlueprints WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Blueprint
            {
                Information                     = item,
                IsCopy                          = reader.GetBoolean (0),
                MaterialLevel                   = reader.GetInt32 (1),
                ProductivityLevel               = reader.GetInt32 (2),
                LicensedProductionRunsRemaining = reader.GetInt32 (3)
            };
        }
    }

    public Corporation LoadCorporation (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT description, tickerName, url, taxRate," +
            " minimumJoinStanding, corporationType, hasPlayerPersonnelManager, sendCharTerminationMessage," +
            " creatorID, ceoID, stationID, raceID, allianceID, shares, memberCount, memberLimit," +
            " allowedMemberRaceIDs, graphicID, shape1, shape2, shape3, color1, color2, color3, typeface," +
            " division1, division2, division3, division4, division5, division6, division7, walletDivision1," +
            " walletDivision2, walletDivision3, walletDivision4, walletDivision5, walletDivision6," +
            " walletDivision7, deleted, startDate, chosenExecutorID " +
            " FROM corporation WHERE corporationID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Corporation
            {
                Description                = reader.GetString (0),
                TickerName                 = reader.GetString (1),
                Url                        = reader.GetString (2),
                TaxRate                    = reader.GetDouble (3),
                MinimumJoinStanding        = reader.GetDouble (4),
                CorporationType            = reader.GetInt32 (5),
                HasPlayerPersonnelManager  = reader.GetBoolean (6),
                SendCharTerminationMessage = reader.GetBoolean (7),
                CreatorID                  = reader.GetInt32 (8),
                CeoID                      = reader.GetInt32 (9),
                StationID                  = reader.GetInt32 (10),
                RaceID                     = reader.GetInt32 (11),
                AllianceID                 = reader.GetInt32OrNull (12),
                Shares                     = reader.GetInt64 (13),
                MemberCount                = reader.GetInt32 (14),
                MemberLimit                = reader.GetInt32 (15),
                AllowedMemberRaceIDs       = reader.GetInt32 (16),
                GraphicId                  = reader.GetInt32 (17),
                Shape1                     = reader.GetInt32OrNull (18),
                Shape2                     = reader.GetInt32OrNull (19),
                Shape3                     = reader.GetInt32OrNull (20),
                Color1                     = reader.GetInt32OrNull (21),
                Color2                     = reader.GetInt32OrNull (22),
                Color3                     = reader.GetInt32OrNull (23),
                Typeface                   = reader.GetStringOrNull (24),
                Division1                  = reader.GetStringOrNull (25),
                Division2                  = reader.GetStringOrNull (26),
                Division3                  = reader.GetStringOrNull (27),
                Division4                  = reader.GetStringOrNull (28),
                Division5                  = reader.GetStringOrNull (29),
                Division6                  = reader.GetStringOrNull (30),
                Division7                  = reader.GetStringOrNull (31),
                WalletDivision1            = reader.GetStringOrNull (32),
                WalletDivision2            = reader.GetStringOrNull (33),
                WalletDivision3            = reader.GetStringOrNull (34),
                WalletDivision4            = reader.GetStringOrNull (35),
                WalletDivision5            = reader.GetStringOrNull (36),
                WalletDivision6            = reader.GetStringOrNull (37),
                WalletDivision7            = reader.GetStringOrNull (38),
                Deleted                    = reader.GetBoolean (39),
                StartDate                  = reader.GetInt64OrNull (40),
                ExecutorCorpID             = reader.GetInt32OrNull (41),
                Information                = item
            };
        }
    }

    public Character LoadCharacter (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT allianceID, accountID, activeCloneID, title, chrInformation.description, securityRating," +
            " petitionMessage, logonMinutes, corporationID, roles, rolesAtBase, rolesAtHQ," +
            " rolesAtOther, corporationDateTime, startDateTime, createDateTime, ancestryID, careerID, schoolID," +
            " careerSpecialityID, gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
            " lipstickID, makeupID, skinID, backgroundID, lightID, headRotation1, headRotation2, headRotation3," +
            " eyeRotation1, eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3, morph1e, morph1n, morph1s," +
            " morph1w, morph2e, morph2n, morph2s, morph2w, morph3e, morph3n, morph3s, morph3w, morph4e, morph4n," +
            " morph4s, morph4w, chrInformation.stationID, solarSystemID, constellationID, regionID, online, freeRespecs, nextRespecTime," +
            " timeLastJump, titleMask, warfactionID, corpAccountKey, " +
            " grantableRoles, grantableRolesAtBase, grantableRolesAtHQ, grantableRolesAtOther, baseID " +
            "FROM chrInformation LEFT JOIN corporation USING(corporationID) WHERE characterID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Character
            {
                Information           = item,
                AllianceID            = reader.GetInt32OrNull (0),
                AccountID             = reader.GetInt32OrDefault (1),
                ActiveCloneID         = reader.GetInt32OrNull (2),
                Title                 = reader.GetString (3),
                Description           = reader.GetString (4),
                SecurityRating        = reader.GetDouble (5),
                PetitionMessage       = reader.GetString (6),
                LogonMinutes          = reader.GetInt32 (7),
                CorporationID         = reader.GetInt32 (8),
                Roles                 = reader.GetInt64 (9),
                RolesAtBase           = reader.GetInt64 (10),
                RolesAtHq             = reader.GetInt64 (11),
                RolesAtOther          = reader.GetInt64 (12),
                CorporationDateTime   = reader.GetInt64 (13),
                StartDateTime         = reader.GetInt64 (14),
                CreateDateTime        = reader.GetInt64 (15),
                AncestryID            = reader.GetInt32 (16),
                CareerID              = reader.GetInt32 (17),
                SchoolID              = reader.GetInt32 (18),
                CareerSpecialityID    = reader.GetInt32 (19),
                Gender                = reader.GetInt32 (20),
                AccessoryID           = reader.GetInt32OrNull (21),
                BeardID               = reader.GetInt32OrNull (22),
                CostumeID             = reader.GetInt32 (23),
                DecoID                = reader.GetInt32OrNull (24),
                EyebrowsID            = reader.GetInt32 (25),
                EyesID                = reader.GetInt32 (26),
                HairID                = reader.GetInt32 (27),
                LipstickID            = reader.GetInt32OrNull (28),
                MakeupID              = reader.GetInt32OrNull (29),
                SkinID                = reader.GetInt32 (30),
                BackgroundID          = reader.GetInt32 (31),
                LightID               = reader.GetInt32 (32),
                HeadRotation1         = reader.GetDouble (33),
                HeadRotation2         = reader.GetDouble (34),
                HeadRotation3         = reader.GetDouble (35),
                EyeRotation1          = reader.GetDouble (36),
                EyeRotation2          = reader.GetDouble (37),
                EyeRotation3          = reader.GetDouble (38),
                CamPos1               = reader.GetDouble (39),
                CamPos2               = reader.GetDouble (40),
                CamPos3               = reader.GetDouble (41),
                Morph1E               = reader.GetDoubleOrNull (42),
                Morph1N               = reader.GetDoubleOrNull (43),
                Morph1S               = reader.GetDoubleOrNull (44),
                Morph1W               = reader.GetDoubleOrNull (45),
                Morph2E               = reader.GetDoubleOrNull (46),
                Morph2N               = reader.GetDoubleOrNull (47),
                Morph2S               = reader.GetDoubleOrNull (48),
                Morph2W               = reader.GetDoubleOrNull (49),
                Morph3E               = reader.GetDoubleOrNull (50),
                Morph3N               = reader.GetDoubleOrNull (51),
                Morph3S               = reader.GetDoubleOrNull (52),
                Morph3W               = reader.GetDoubleOrNull (53),
                Morph4E               = reader.GetDoubleOrNull (54),
                Morph4N               = reader.GetDoubleOrNull (55),
                Morph4S               = reader.GetDoubleOrNull (56),
                Morph4W               = reader.GetDoubleOrNull (57),
                StationID             = reader.GetInt32 (58),
                SolarSystemID         = reader.GetInt32 (59),
                ConstellationID       = reader.GetInt32 (60),
                RegionID              = reader.GetInt32 (61),
                FreeReSpecs           = reader.GetInt32 (63),
                NextReSpecTime        = reader.GetInt64 (64),
                TimeLastJump          = reader.GetInt64 (65),
                TitleMask             = reader.GetInt32 (66),
                WarFactionID          = reader.GetInt32OrNull (67),
                CorpAccountKey        = reader.GetInt32 (68),
                GrantableRoles        = reader.GetInt64 (69),
                GrantableRolesAtBase  = reader.GetInt64 (70),
                GrantableRolesAtHQ    = reader.GetInt64 (71),
                GrantableRolesAtOther = reader.GetInt64 (72),
                BaseID                = reader.GetInt32OrNull (73)
            };
        }
    }

    public Faction LoadFaction (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT description, raceIDs, solarSystemID, corporationID, sizeFactor, stationCount," +
            " stationSystemCount, militiaCorporationID" +
            " FROM chrFactions",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Faction
            {
                Description          = reader.GetString (0),
                RaceIDs              = reader.GetInt32 (1),
                SolarSystemID        = reader.GetInt32 (2),
                CorporationID        = reader.GetInt32 (3),
                SizeFactor           = reader.GetDouble (4),
                StationCount         = reader.GetInt32 (5),
                StationSystemCount   = reader.GetInt32 (6),
                MilitiaCorporationID = reader.GetInt32 (7),
                Information          = item
            };
        }
    }

    public Alliance LoadAlliance (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT shortName, description, url, executorCorpID, creatorCorpID, creatorCharID, dictatorial FROM crpAlliances WHERE allianceID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Alliance
            {
                ShortName      = reader.GetString (0),
                Description    = reader.GetString (1),
                URL            = reader.GetString (2),
                ExecutorCorpID = reader.GetInt32OrNull (3),
                CreatorCorpID  = reader.GetInt32 (4),
                CreatorCharID  = reader.GetInt32 (5),
                Dictatorial    = reader.GetBoolean (6),
                Information    = item
            };
        }
    }

    public Station LoadStation (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT operationID, security, dockingCostPerVolume, maxShipVolumeDockable, officeRentalCost, constellationID, regionID, reprocessingEfficiency, reprocessingStationsTake, reprocessingHangarFlag FROM staStations WHERE stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Station
            {
                Type                     = StationManager.StationTypes [item.Type.ID],
                Operations               = StationManager.Operations [reader.GetInt32 (0)],
                Security                 = reader.GetInt32 (1),
                DockingCostPerVolume     = reader.GetDouble (2),
                MaxShipVolumeDockable    = reader.GetDouble (3),
                OfficeRentalCost         = reader.GetInt32 (4),
                ConstellationID          = reader.GetInt32 (5),
                RegionID                 = reader.GetInt32 (6),
                ReprocessingEfficiency   = reader.GetDouble (7),
                ReprocessingStationsTake = reader.GetDouble (8),
                ReprocessingHangarFlag   = reader.GetInt32 (9),
                Information              = item
            };
        }
    }

    private void SaveItemName (ulong itemID, Type type, string itemName)
    {
        // save item name if exists
        Database.Prepare (
            "REPLACE INTO eveNames (itemID, itemName, categoryID, groupID, typeID)VALUES(@itemID, @itemName, @categoryID, @groupID, @typeID)",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@itemName", itemName},
                {"@categoryID", type.Group.Category.ID},
                {"@groupID", type.Group.ID},
                {"@typeID", type.ID}
            }
        );
    }

    private void SaveItemPosition (ulong itemID, double? x, double? y, double? z)
    {
        Database.Prepare (
            "REPLACE INTO invPositions (itemID, x, y, z)VALUES(@itemID, @x, @y, @z)",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@x", x},
                {"@y", y},
                {"@z", z}
            }
        );
    }

    private void SaveItemName (ItemEntity item)
    {
        this.SaveItemName ((ulong) item.ID, item.Type, item.Name);
    }

    private void SaveItemPosition (ItemEntity item)
    {
        this.SaveItemPosition ((ulong) item.ID, item.X, item.Y, item.Z);
    }

    public ulong CreateItem (
        string itemName,   Type type,      ItemEntity owner,    ItemEntity location, Flags   flag,
        bool   contraband, bool singleton, int        quantity, double?    x,        double? y, double? z, string customInfo
    )
    {
        ulong newItemID = Database.PrepareLID (
            "INSERT INTO invItems(itemID, typeID, ownerID, locationID, flag, contraband, singleton, quantity, customInfo)VALUES(NULL, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @customInfo)",
            new Dictionary <string, object>
            {
                {"@typeID", type.ID},
                {"@ownerID", owner?.ID},
                {"@locationID", location?.ID},
                {"@flag", flag},
                {"@contraband", contraband},
                {"@singleton", singleton},
                {"@quantity", quantity},
                {"@x", x},
                {"@y", y},
                {"@z", z},
                {"@customInfo", customInfo}
            }
        );

        if (itemName is not null)
            this.SaveItemName (newItemID, type, itemName);
        if (x is not null && y is not null && z is not null)
            this.SaveItemPosition (newItemID, x, y, z);

        return newItemID;
    }

    public ulong CreateItem (
        string itemName,  int typeID,   int     owner, int?    location, Flags   flag, bool   contraband,
        bool   singleton, int quantity, double? x,     double? y,        double? z,    string customInfo
    )
    {
        ulong newItemID = Database.PrepareLID (
            "INSERT INTO invItems(itemID, typeID, ownerID, locationID, flag, contraband, singleton, quantity, customInfo)VALUES(NULL, @typeID, @ownerID, @locationID, @flag, @contraband, @singleton, @quantity, @customInfo)",
            new Dictionary <string, object>
            {
                {"@typeID", typeID},
                {"@ownerID", owner},
                {"@locationID", location},
                {"@flag", flag},
                {"@contraband", contraband},
                {"@singleton", singleton},
                {"@quantity", quantity},
                {"@customInfo", customInfo}
            }
        );

        if (itemName is not null)
            this.SaveItemName (newItemID, this.Types [typeID], itemName);
        if (x is not null && y is not null && z is not null)
            this.SaveItemPosition (newItemID, x, y, z);

        return newItemID;
    }

    public ulong CreateShip (Type shipType, ItemEntity location, ItemEntity owner)
    {
        return this.CreateItem (
            $"{owner.Name}'s {shipType.Name}", shipType, owner, location, Flags.Hangar,
            false, true, 1, null, null, null, null
        );
    }

    public IEnumerable <int> LoadItemsLocatedAt (int locationID, Flags ignoreFlag)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID FROM invItems WHERE locationID = @locationID AND flag != @flag",
            new Dictionary <string, object>
            {
                {"@locationID", locationID},
                {"@flag", ignoreFlag}
            }
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                yield return reader.GetInt32 (0);
        }
    }

    public IEnumerable <int> LoadItemsLocatedAtByOwner (int locationID, int ownerID, Flags itemFlag)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID FROM invItems WHERE locationID = @locationID AND ownerID = @ownerID AND flag = @flag",
            new Dictionary <string, object>
            {
                {"@locationID", locationID},
                {"@ownerID", ownerID},
                {"@flag", (int) itemFlag}
            }
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                yield return reader.GetInt32 (0);
        }
    }

    public SolarSystem LoadSolarSystem (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT regionID, constellationID, x, y, z, xMin, yMin, zMin, xMax, yMax, zMax, luminosity, border, fringe, corridor, hub, international, regional, constellation, security, factionID, radius, sunTypeID, securityClass FROM mapSolarSystems WHERE solarSystemID = @solarSystemID",
            new Dictionary <string, object> {{"@solarSystemID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new SolarSystem
            {
                RegionId        = reader.GetInt32 (0),
                ConstellationId = reader.GetInt32 (1),
                MapX            = reader.GetDouble (2),
                MapY            = reader.GetDouble (3),
                MapZ            = reader.GetDouble (4),
                MapXMin         = reader.GetDouble (5),
                MapYMin         = reader.GetDouble (6),
                MapZMin         = reader.GetDouble (7),
                MapXMax         = reader.GetDouble (8),
                MapYMax         = reader.GetDouble (9),
                MapZMax         = reader.GetDouble (10),
                Luminosity      = reader.GetDouble (11),
                Border          = reader.GetBoolean (12),
                Fringe          = reader.GetBoolean (13),
                Corridor        = reader.GetBoolean (14),
                Hub             = reader.GetBoolean (15),
                International   = reader.GetBoolean (16),
                Regional        = reader.GetBoolean (17),
                Constellation   = reader.GetBoolean (18),
                Security        = reader.GetDouble (19),
                FactionId       = reader.GetInt32OrNull (20),
                Radius          = reader.GetDouble (21),
                SunTypeId       = reader.GetInt32 (22),
                SecurityClass   = reader.GetStringOrNull (23),
                Information     = item
            };
        }
    }

    public Constellation LoadConstellation (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT regionID, x, y, z, xMin, yMin, zMin, xMax, yMax, zMax, factionID, radius FROM mapConstellations WHERE constellationID = @constellationID",
            new Dictionary <string, object> {{"@constellationID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Constellation
            {
                RegionId    = reader.GetInt32 (0),
                X           = reader.GetDouble (1),
                Y           = reader.GetDouble (2),
                Z           = reader.GetDouble (3),
                XMin        = reader.GetDouble (4),
                YMin        = reader.GetDouble (5),
                ZMin        = reader.GetDouble (6),
                XMax        = reader.GetDouble (7),
                YMax        = reader.GetDouble (8),
                ZMax        = reader.GetDouble (9),
                FactionId   = reader.GetInt32OrNull (10),
                Radius      = reader.GetDouble (11),
                Information = item
            };
        }
    }

    public Region LoadRegion (Item item)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT xMin, yMin, zMin, xMax, yMax, zMax, factionID, radius FROM mapRegions WHERE regionID = @regionID",
            new Dictionary <string, object> {{"@regionID", item.ID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Region
            {
                XMin        = reader.GetDouble (0),
                YMin        = reader.GetDouble (1),
                ZMin        = reader.GetDouble (2),
                XMax        = reader.GetDouble (3),
                YMax        = reader.GetDouble (4),
                ZMax        = reader.GetDouble (5),
                FactionID   = reader.GetInt32OrNull (6),
                Radius      = reader.GetDouble (7),
                Information = item
            };
        }
    }

    public void UnloadItem (int itemID)
    {
        // non-user generated items are not owned by anyone
        if (itemID < ItemRanges.USERGENERATED_ID_MIN)
            return;

        Database.Prepare ("UPDATE invItems SET nodeID = 0 WHERE itemID = @itemID", new Dictionary <string, object> {{"@itemID", itemID}});
    }

    private Dictionary <int, Attribute> LoadAttributesForItem (int itemID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT attributeID, valueInt, valueFloat FROM invItemsAttributes WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, Attribute> result = new Dictionary <int, Attribute> ();

            while (reader.Read ())
            {
                Attribute attribute = null;

                if (reader.IsDBNull (1))
                    attribute = new Attribute (
                        AttributeManager [reader.GetInt32 (0)],
                        reader.GetDouble (2)
                    );
                else
                    attribute = new Attribute (
                        AttributeManager [reader.GetInt32 (0)],
                        reader.GetInt64 (1)
                    );

                result [attribute.ID] = attribute;
            }

            return result;
        }
    }

    /// <summary>
    /// Saves an entity to the database
    /// </summary>
    /// <param name="item"></param>
    public void PersistEntity (ItemEntity item)
    {
        Database.Prepare (
            "UPDATE invItems SET typeID = @typeID, ownerID = @ownerID, locationID = @locationID, flag = @flag, contraband = @contraband, singleton = @singleton, quantity = @quantity, customInfo = @customInfo WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemName", item.Name},
                {"@typeID", item.Type.ID},
                {"@ownerID", item.OwnerID},
                {"@locationID", item.LocationID},
                {"@flag", item.Flag},
                {"@contraband", item.Contraband},
                {"@singleton", item.Singleton},
                {"@quantity", item.Quantity},
                {"@customInfo", item.CustomInfo},
                {"@itemID", item.ID}
            }
        );

        // ensure naming information is up to date
        if (item.HasName)
            this.SaveItemName (item);
        else if (item.HadName)
            Database.Prepare (
                "DELETE FROM eveNames WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", item.ID}}
            );

        if (item.HasPosition)
            this.SaveItemPosition (item);
        else if (item.HadPosition)
            Database.Prepare (
                "DELETE FROM invPositions WHERE itemID = @itemID",
                new Dictionary <string, object> {{"@itemID", item.ID}}
            );
    }

    public void PersistAttributeList (ItemEntity item, AttributeList list)
    {
        IDbConnection connection = null;
        MySqlCommand command = (MySqlCommand) Database.Prepare (
            ref connection,
            "REPLACE INTO invItemsAttributes(itemID, attributeID, valueInt, valueFloat) VALUE (@itemID, @attributeID, @valueInt, @valueFloat)"
        );

        using (connection)
        using (command)
        {
            foreach (KeyValuePair <int, Attribute> pair in list)
            {
                // only update dirty records
                if (pair.Value.Dirty == false && pair.Value.New == false)
                    continue;

                command.Parameters.Clear ();

                command.Parameters.AddWithValue ("@itemID",      item.ID);
                command.Parameters.AddWithValue ("@attributeID", pair.Value.ID);

                if (pair.Value.ValueType == Attribute.ItemAttributeValueType.Integer)
                    command.Parameters.AddWithValue ("@valueInt", pair.Value.Integer);
                else
                    command.Parameters.AddWithValue ("@valueInt", null);

                if (pair.Value.ValueType == Attribute.ItemAttributeValueType.Double)
                    command.Parameters.AddWithValue ("@valueFloat", pair.Value.Float);
                else
                    command.Parameters.AddWithValue ("@valueFloat", null);

                command.ExecuteNonQuery ();

                pair.Value.New   = false;
                pair.Value.Dirty = false;
            }
        }
    }

    public void PersistBlueprint (Blueprint information)
    {
        Database.Prepare (
            "UPDATE invBlueprints SET copy = @copy, materialLevel = @materialLevel, productivityLevel = @productivityLevel, licensedProductionRunsRemaining = @licensedProductionRunsRemaining WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", information.Information.ID},
                {"@copy", information.IsCopy},
                {"@materialLevel", information.MaterialLevel},
                {"@productivityLevel", information.ProductivityLevel},
                {"@licensedProductionRunsRemaining", information.LicensedProductionRunsRemaining}
            }
        );
    }

    public CRowset ListStations (int ownerID, int blueprintsOnly)
    {
        return Database.PrepareCRowset (
            "SELECT stationID, COUNT(itemID) AS itemCount, COUNT(invBlueprints.itemID) AS blueprintCount " +
            "FROM staStations " +
            "LEFT JOIN invItems ON locationID = stationID " +
            "LEFT JOIN invBlueprints USING(itemID) " +
            "WHERE ownerID=@characterID AND flag=@hangarFlag " +
            "GROUP BY stationID " +
            "HAVING blueprintCount >= @minimumBlueprintCount",
            new Dictionary <string, object>
            {
                {"@characterID", ownerID},
                {"@hangarFlag", Flags.Hangar},
                {"@minimumBlueprintCount", blueprintsOnly}
            }
        );
    }

    public CRowset ListStationItems (int locationID, int ownerID)
    {
        return Database.PrepareCRowset (
            "SELECT itemID, typeID, locationID, ownerID, flag, contraband, singleton, quantity, groupID, categoryID " +
            "FROM invItems " +
            "LEFT JOIN invTypes USING(typeID) " +
            "LEFT JOIN invGroups USING(groupID) " +
            "WHERE ownerID=@ownerID AND locationID=@locationID AND flag=@hangarFlag",
            new Dictionary <string, object>
            {
                {"@ownerID", ownerID},
                {"@locationID", locationID},
                {"@hangarFlag", Flags.Hangar}
            }
        );
    }

    public Rowset ListStationBlueprintItems (int locationID, int ownerID)
    {
        return Database.PrepareRowset (
            "SELECT itemID, typeID, locationID, ownerID, flag, contraband, singleton, quantity, groupID, categoryID, copy, productivityLevel, materialLevel, licensedProductionRunsRemaining FROM invItems LEFT JOIN invTypes USING (typeID) LEFT JOIN invGroups USING (groupID) LEFT JOIN invBlueprints USING (itemID) WHERE ownerID = @ownerID AND locationID = @locationID AND categoryID = @blueprintCategoryID",
            new Dictionary <string, object>
            {
                {"@ownerID", ownerID},
                {"@locationID", locationID},
                {"@blueprintCategoryID", EVE.Data.Inventory.CategoryID.Blueprint}
            }
        );
    }

    public Rowset GetClonesForCharacter (int characterID, int activeCloneID)
    {
        // TODO: CACHE THIS IN A INTERMEDIATE TABLE TO MAKE THINGS EASIER TO QUERY
        return Database.PrepareRowset (
            "SELECT itemID AS jumpCloneID, typeID, locationID FROM invItems WHERE flag = @cloneFlag AND ownerID = @characterID AND itemID != @activeCloneID AND locationID IN(SELECT stationID FROM staStations)",
            new Dictionary <string, object>
            {
                {"@cloneFlag", Flags.Clone},
                {"@activeCloneID", activeCloneID},
                {"@characterID", characterID}
            }
        );
    }

    public Rowset GetClonesInShipForCharacter (int characterID)
    {
        return Database.PrepareRowset (
            "SELECT itemID AS jumpCloneID, typeID, locationID FROM invItems WHERE flag = @cloneFlag AND ownerID = @characterID AND locationID NOT IN(SELECT stationID FROM staStations)",
            new Dictionary <string, object>
            {
                {"@cloneFlag", Flags.Clone},
                {"@characterID", characterID}
            }
        );
    }

    public Rowset GetImplantsForCharacterClones (int characterID)
    {
        return Database.PrepareRowset (
            "SELECT invItems.itemID, invItems.typeID, invItems.locationID as jumpCloneID FROM invItems LEFT JOIN invItems second ON invItems.locationID = second.itemID  WHERE invItems.flag = @implantFlag AND second.flag = @cloneFlag AND second.ownerID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@implantFlag", Flags.Implant},
                {"@cloneFlag", Flags.Clone}
            }
        );
    }

    public void DestroyItem (ItemEntity item)
    {
        Database.Prepare (
            "DELETE FROM invItems WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );
        Database.Prepare (
            "DELETE FROM eveNames WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );
        Database.Prepare (
            "DELETE FROM invItemsAttributes WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );
    }

    public void UpdateItemLocation (int itemID, int newLocationID)
    {
        Database.Prepare (
            "UPDATE invItems SET locationID = @locationID WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@locationID", newLocationID}
            }
        );
    }

    public void UpdateItemOwner (int itemID, int newOwnerID)
    {
        Database.Prepare (
            "UPDATE invItems SET ownerID = @ownerID WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@ownerID", newOwnerID}
            }
        );
    }

    public void UpdateItemQuantity (int itemID, int newQuantity)
    {
        Database.Prepare (
            "UPDATE invItems SET quantity = @quantity WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@quantity", newQuantity}
            }
        );
    }

    public int GetItemNode (int itemID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT nodeID FROM invItems WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", itemID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }
}