using System.Data.Common;
using EVESharp.API.Models;
using EVESharp.Database;
using Microsoft.AspNetCore.Mvc;

namespace EVESharp.API.Controllers;

[ApiController]
[Route("/account/")]
public class AccountController : ControllerBase
{
    private readonly IDatabase DB;

    public AccountController (IDatabase db)
    {
        DB = db;
    }

    [Route ("Characters.xml.aspx")]
    [HttpPost]
    public ActionResult <EVEApiModel <RowsetModel <CharacterRow>>> Characters ()
    {
        // TODO: ALLOW THE API TO SPECIFY THE PLAYER
        DbDataReader reader = DB.Select (
            "SELECT characterID, corporationID, chr.itemName, crp.itemName FROM chrInformation LEFT JOIN eveNames chr ON chr.itemID = characterID LEFT JOIN eveNames crp ON crp.itemID = corporationID WHERE accountID = @accountID;",
            new Dictionary <string, object> ()
            {
                {"@accountID", 4}
            }
        );

        using (reader)
        {
            RowsetModel <CharacterRow> result = new RowsetModel <CharacterRow>
            {
                Rowset = new Rowset <CharacterRow> ()
                {
                    Columns = "name,characterID,corporationName,corporationID",
                    Key = "characterID",
                    Name = "characters"
                }
            };

            while (reader.Read () == true)
            {
                CharacterRow row = new CharacterRow
                {
                    CharacterID     = reader.GetInt32 (0),
                    CorporationID   = reader.GetInt32 (1),
                    Name            = reader.GetString (2),
                    CorporationName = reader.GetString (3)
                };

                result.Rowset.Rows.Add (row);
            }

            return new EVEApiModel <RowsetModel <CharacterRow>> ()
            {
                Result = result
            };
        }
    }
}