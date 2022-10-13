using System.Data;
using System.Data.Common;
using EVESharp.API.Models;
using EVESharp.Database;
using EVESharp.Database.Configuration;
using EVESharp.Database.EVEMath;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Market;
using EVESharp.Database.Types;
using Microsoft.AspNetCore.Mvc;

namespace EVESharp.API.Controllers;

[ApiController]
[Route("/char/")]
public class CharacterController : ControllerBase
{
    private readonly IDatabase  DB;
    private readonly IConstants Constants;

    public CharacterController (IDatabase db, IConstants constants)
    {
        DB        = db;
        Constants = constants;
    }

    [Route("AccountBalance.xml.aspx")]
    [HttpPost]
    public ActionResult <EVEApiModel<RowsetModel<AccountRow>>> AccountBalance ([FromForm] long characterID)
    {
        // TODO: PERFORM PERMISSION CHECKS
        DbDataReader reader = DB.Select (
            "SELECT `key`, balance FROM mktWallets WHERE ownerID = @ownerID",
            new Dictionary <string, object> ()
            {
                {"@ownerID", characterID}
            }
        );

        using (reader)
        {
            RowsetModel <AccountRow> result = new RowsetModel <AccountRow> ()
            {
                Rowset = new Rowset <AccountRow> ()
                {
                    Columns = "accountID,accountKey,balance",
                    Key     = "accountID",
                    Name    = "accounts"
                }
            };

            while (reader.Read () == true)
            {
                // EVESharp uses the accountKey as accountID
                AccountRow row = new AccountRow
                {
                    Balance    = reader.GetDouble (1),
                    AccountID  = reader.GetInt32 (0),
                    AccountKey = reader.GetInt32 (0)
                };

                result.Rowset.Rows.Add (row);
            }

            return new EVEApiModel <RowsetModel <AccountRow>> ()
            {
                Result = result
            };
        }
    }

    private Rowset <CertificateRow> GetCertificates (int characterID)
    {
        DbDataReader reader = DB.Select (
            "SELECT certificateID FROM chrCertificates WHERE characterID = @characterID",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID}
            }
        );

        using (reader)
        {
            Rowset <CertificateRow> result = new Rowset <CertificateRow> ()
            {
                Columns = "certificateID",
                Key     = "certificateID",
                Name    = "certificates"
            };

            while (reader.Read () == true)
            {
                CertificateRow row = new CertificateRow
                {
                    CertificateID = reader.GetInt32 (0)
                };

                result.Rows.Add (row);
            }

            return result;
        }
    }

    private Rowset <CorporationRoleRow> GetRoles (int characterID, string field, string name)
    {
        DbDataReader reader = DB.Select (
            $"SELECT roleID, roleName FROM crpRoles LEFT JOIN chrInformation ON {field} & roleID = roleID WHERE characterID = @characterID;",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID}
            }
        );

        using (reader)
        {
            Rowset <CorporationRoleRow> result = new Rowset <CorporationRoleRow> ()
            {
                Columns = "roleID,roleName",
                Key     = "roleID",
                Name    = name
            };

            while (reader.Read () == true)
            {
                CorporationRoleRow row = new CorporationRoleRow ()
                {
                    RoleID   = reader.GetInt64 (0),
                    RoleName = reader.GetString (1)
                };
                
                result.Rows.Add (row);
            }

            return result;
        }
    }

    private Rowset <TitleRow> GetTitles (int characterID)
    {
        DbDataReader reader = DB.Select (
            @"SELECT
                titleID, titleName
            FROM crpTitles
            LEFT JOIN chrInformation ON titleMask & titleID = titleID AND crpTitles.corporationID = chrInformation.corporationID
            WHERE characterID = @characterID;",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID}
            }
        );

        using (reader)
        {
            Rowset <TitleRow> result = new Rowset <TitleRow> ()
            {
                Columns = "titleID,titleName",
                Key     = "titleID",
                Name    = "corporationTitles"
            };

            while (reader.Read () == true)
            {
                TitleRow row = new TitleRow ()
                {
                    ID   = reader.GetInt64 (0),
                    Name = reader.GetString (1)
                };
                
                result.Rows.Add (row);
            }

            return result;
        }
    }

    private Rowset <SkillRow> GetSkills (int characterID)
    {
        DbDataReader reader = DB.Select(
            @"SELECT
	            typeID, level.valueInt, COALESCE (points.valueInt, points.valueFloat), published
            FROM invItems i
            LEFT JOIN invItemsAttributes level ON level.itemID = i.itemID AND level.attributeID = @level
            LEFT JOIN invItemsAttributes points ON points.itemID = i.itemID AND points.attributeID = @points
            LEFT JOIN invTypes USING (typeID)
            WHERE locationID = @characterID AND (flag = @skillFlag OR flag = @skillInTrainingFlag);",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID},
                {"@level", AttributeTypes.skillLevel},
                {"@points", AttributeTypes.skillPoints},
                {"@skillFlag", Flags.Skill},
                {"@skillInTrainingFlag", Flags.SkillInTraining}
            }
        );

        using (reader)
        {
            Rowset <SkillRow> result = new Rowset <SkillRow> ()
            {
                Columns = "typeID,skillpoints,level,published",
                Key     = "typeID",
                Name    = "skills"
            };

            while (reader.Read () == true)
            {
                SkillRow row = new SkillRow ()
                {
                    TypeID      = reader.GetInt32 (0),
                    SkillPoints = reader.GetDouble (2),
                    Level       = reader.GetInt32 (1),
                    Published   = reader.GetBoolean (3)
                };

                result.Rows.Add (row);
            }

            return result;
        }
    }

    [Route("CharacterSheet.xml.aspx")]
    [HttpPost]
    public ActionResult <EVEApiModel <CharacterModel>> CharacterSheet ([FromForm] int characterID)
    {
        // TODO: PERFORM PERMISSION CHECKS
        DbDataReader reader = DB.Select (
            @"SELECT
	            characterID, chr.itemName, crp.itemName, ali.itemName, startDateTime, warfactionID, factionName,
                cloneType.typeName, cloneAttrib.valueInt, intelligence.valueInt, memory.valueInt, charisma.valueInt, perception.valueInt, willpower.valueInt,
                raceName, ancestryName, mktWallets.balance, allianceID, bloodlineName, gender, c.corporationID
            FROM chrInformation c
            LEFT JOIN corporation USING (corporationID)
            LEFT JOIN eveNames chr ON chr.itemID = characterID
            LEFT JOIN eveNames crp ON crp.itemID = corporationID
            LEFT JOIN eveNames ali ON ali.itemID = corporation.allianceID
            LEFT JOIN chrRaces USING (raceID)
            LEFT JOIN chrAncestries USING (ancestryID)
            LEFT JOIN chrFactions ON factionID = warfactionID
            LEFT JOIN chrBloodlines USING (bloodlineID)
            LEFT JOIN invItems clone ON clone.itemID = activeCloneID
            LEFT JOIN invTypes cloneType ON cloneType.typeID = clone.typeID 
            LEFT JOIN dgmTypeAttributes cloneAttrib ON (cloneAttrib.typeID = clone.typeID AND cloneAttrib.attributeID = @skillPoints)
            LEFT JOIN invItemsAttributes intelligence ON (intelligence.itemID = characterID AND intelligence.attributeID = @intelligence)
            LEFT JOIN invItemsAttributes memory ON (memory.itemID = characterID AND memory.attributeID = @memory)
            LEFT JOIN invItemsAttributes charisma ON (charisma.itemID = characterID AND charisma.attributeID = @charisma)
            LEFT JOIN invItemsAttributes perception ON (perception.itemID = characterID AND perception.attributeID = @perception)
            LEFT JOIN invItemsAttributes willpower ON (willpower.itemID = characterID AND willpower.attributeID = @willpower)
            LEFT JOIN mktWallets ON (mktWallets.ownerID = characterID AND `key` = @walletKey)
            WHERE characterID = @characterID",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID},
                {"@skillPoints", AttributeTypes.skillPointsSaved},
                {"@intelligence", AttributeTypes.intelligence},
                {"@memory", AttributeTypes.memory},
                {"@charisma", AttributeTypes.charisma},
                {"@perception", AttributeTypes.perception},
                {"@willpower", AttributeTypes.willpower},
                {"@walletKey", WalletKeys.MAIN}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Cannot find the given character");
            
            CharacterModel result = new CharacterModel ()
            {
                CharacterID = characterID,
                Name = reader.GetString (1),
                DateOfBirth = DateTime.FromFileTimeUtc (reader.GetInt64 (4)),
                Race = reader.GetString (14),
                Ancestry = reader.GetString (15),
                AllianceID = reader.GetInt32OrDefault (17),
                AllianceName = reader.GetStringOrNull (3),
                Attributes = new AttributesModel ()
                {
                    Charisma = reader.GetInt32 (11),
                    Memory = reader.GetInt32 (10),
                    Intelligence = reader.GetInt32 (9),
                    Perception = reader.GetInt32 (12),
                    Willpower = reader.GetInt32 (13)
                },
                Balance = reader.GetDouble (16),
                Bloodline = reader.GetString (18),
                Gender = reader.GetBoolean (19) ? "Male" : "Female",
                CloneName = reader.GetString (7),
                CorporationName = reader.GetString (2),
                CorporationID = reader.GetInt32 (20),
                CloneSkillPoints = reader.GetInt32 (8),
                FactionName = reader.GetStringOrNull (6),
                FactionID = reader.GetInt32OrDefault (5),
                Certificates = GetCertificates (characterID),
                Skills = GetSkills (characterID),
                CorporationRoles = GetRoles (characterID, "roles", "corporationRoles"),
                CorporationRolesAtHQ = GetRoles (characterID, "rolesAtHQ", "corporationRolesAtHQ"),
                CorporationRolesAtBase = GetRoles (characterID, "rolesAtBase", "corporationRolesAtBase"),
                CorporationRolesAtOther = GetRoles (characterID, "rolesAtOther", "corporationRolesAtOther"),
                Titles = GetTitles (characterID)
            };

            return new EVEApiModel <CharacterModel> ()
            {
                Result = result
            };
        }
    }

    [Route ("SkillQueue.xml.aspx")]
    [HttpPost]
    public ActionResult <EVEApiModel<RowsetModel<SkillQueueRow>>> SkillQueue ([FromForm] int characterID)
    {
        DbDataReader reader = DB.Select(
            @"SELECT
	            orderIndex, level, COALESCE(constant.valueFloat, constant.valueInt), COALESCE(points.valueFloat, points.valueInt),
                i.typeID, COALESCE(expiry.valueFloat, expiry.valueInt)
            FROM chrSkillQueue
            LEFT JOIN invItems i ON i.itemID = skillItemID
            LEFT JOIN invItemsAttributes constant ON constant.itemID = skillItemID AND constant.attributeID = @constantAttribute
            LEFT JOIN invItemsAttributes points ON points.itemID = skillItemID AND points.attributeID = @pointsAttribute
            LEFT JOIN invItemsAttributes expiry ON expiry.itemID = skillItemID AND expiry.attributeID = @expiryAttribute;",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID},
                {"@constantAttribute", AttributeTypes.skillTimeConstant},
                {"@pointsAttribute", AttributeTypes.skillPoints},
                {"@expiryAttribute", AttributeTypes.expiryTime}
            }
        );

        using (reader)
        {
            RowsetModel <SkillQueueRow> result = new RowsetModel <SkillQueueRow> ()
            {
                Rowset = new Rowset <SkillQueueRow> ()
                {
                    Columns = "queuePosition,typeID,level,startSP,endSP,startTime,endTime",
                    Key     = "queuePosition",
                    Name    = "skillqueue"
                }
            };

            while (reader.Read () == true)
            {
                SkillQueueRow row = new SkillQueueRow ()
                {
                    Level    = reader.GetInt32 (1),
                    Position = reader.GetInt32 (0) + 1,
                    TypeID = reader.GetInt32 (4),
                    StartSP  = reader.GetDouble (3),
                    EndSP  = Skills.GetSkillPointsForLevel (reader.GetInt32 (1), reader.GetDouble (2), Constants.SkillPointMultiplier),
                    StartTime = DateTime.Now,
                    EndTime = DateTime.FromFileTimeUtc (reader.GetInt64 (5))
                };
                
                // TODO: CALCULATE SOME START TIME SO THE UI DISPLAYS THE RIGHT STUFF

                result.Rowset.Rows.Add (row);
            }
            
            return new EVEApiModel <RowsetModel <SkillQueueRow>> ()
            {
                Result = result
            };
        }
    }

    [Route ("MarketOrders.xml.aspx")]
    [HttpPost]
    public ActionResult <EVEApiModel <RowsetModel <MarketOrderRow>>> MarketOrders ([FromForm] int characterID)
    {
        DbDataReader reader = DB.Select(
            @"SELECT
                orderID, charID, stationID, volEntered, volRemaining, minVolume,
                typeID, `range`, accountID, duration, escrow, price, bid, issued
            FROM mktOrders
            WHERE charID = @characterID",
            new Dictionary <string, object> ()
            {
                {"@characterID", characterID},
            }
        );

        using (reader)
        {
            RowsetModel <MarketOrderRow> result = new RowsetModel <MarketOrderRow> ()
            {
                Rowset = new Rowset <MarketOrderRow> ()
                {
                    Columns = "orderID,charID,stationID,volEntered,volRemaining,minVolume,orderState,typeID,range,accountKey,duration,escrow,price,bid,issued",
                    Key     = "orderID",
                    Name    = "orders"
                }
            };

            while (reader.Read () == true)
            {
                MarketOrderRow row = new MarketOrderRow
                {
                    OrderID = reader.GetInt32 (0),
                    CharacterID = reader.GetInt32 (1),
                    StationID = reader.GetInt32 (2),
                    VolEntered = reader.GetDouble (3),
                    VolRemaining = reader.GetDouble (4),
                    MinVolume = reader.GetDouble (5),
                    OrderState = 0, // we do not keep states, only active orders
                    TypeID = reader.GetInt32 (6),
                    Range = reader.GetInt32(7),
                    AccountKey = reader.GetInt32 (8),
                    Duration = reader.GetInt32 (9),
                    Escrow = reader.GetDouble (10),
                    Price = reader.GetInt32 (11),
                    Bid = reader.GetBoolean (12),
                    Issued = DateTime.FromFileTimeUtc (reader.GetInt64(13))
                };
                
                // TODO: CALCULATE SOME START TIME SO THE UI DISPLAYS THE RIGHT STUFF

                result.Rowset.Rows.Add (row);
            }
            
            return new EVEApiModel <RowsetModel <MarketOrderRow>> ()
            {
                Result = result
            };
        }
    }
}