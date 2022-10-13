using System.Data.Common;
using EVESharp.Database;
using Microsoft.AspNetCore.Mvc;

namespace EVESharp.API.Controllers;

[ApiController]
[Route ("/")]
public class ImagesController : ControllerBase
{
    private readonly IDatabase DB;

    public ImagesController (IDatabase db)
    {
        DB = db;
    }
    
    private string? ResolvePath (int size, int id)
    {
        while (size > 32)
        {
            string path = Path.Combine (Directory.GetCurrentDirectory (), $"res/types/{size}", $"{id}.png");

            if (System.IO.File.Exists (path) == true)
                return path;

            size >>= 1;
        }

        return null;
    }
    
    [Route ("images/characters/{size}/{id}")]
    [HttpGet]
    public IActionResult Characters (int size, int id)
    {
        // for now just return the file
        return PhysicalFile (Path.Combine (Directory.GetCurrentDirectory (), "res", "portrait.jpg"), "image/jpeg");
    }

    [Route ("images/types/{size}/{id}")]
    [HttpGet]
    public IActionResult Types (int size, int id)
    {
        // first check if there's an image with the given size for the given itemID first
        // otherwise try to look up the icon in the database
        string? path = ResolvePath (size, id);

        if (path is not null)
            return PhysicalFile (path, "image/png");

        // check the database for the icon file
        DbDataReader reader = DB.Select (
            "SELECT icon FROM invTypes RIGHT JOIN eveGraphics USING (graphicID) WHERE typeID = @typeID;",
            new Dictionary <string, object> ()
            {
                {"@typeID", id}
            }
        );

        if (reader.Read () == true)
        {
            string icon = reader.GetStringOrNull (0);

            if (string.IsNullOrEmpty (icon) == false)
            {
                path = Path.Combine (Directory.GetCurrentDirectory (), "res/types/icons/", $"icon{icon}.png");

                if (System.IO.File.Exists (path) == true)
                    return PhysicalFile (path, "image/png");
            }
        }
        
        // last resort, return a placeholder
        return PhysicalFile (Path.Combine (Directory.GetCurrentDirectory (), "res/types/placeholder.png"), "image/png");
    }

    [Route("icons/{size}/{file}.png")]
    [HttpGet]
    public IActionResult OldIconsTanslate (string size, string file)
    {
        // trying to find an icon, locate it and return
        if (file.StartsWith ("icon") == true)
        {
            string path = Path.Combine (Directory.GetCurrentDirectory (), "res/types/icons/", $"{file}.png");

            if (System.IO.File.Exists (path) == true)
                return PhysicalFile (path, "image/png");
            
            return PhysicalFile (Path.Combine (Directory.GetCurrentDirectory (), "res/types/placeholder.png"), "image/png");
        }
        
        // normal icon, parse sizes and call the better approach
        if (size.Contains ("_") == false)
            return BadRequest ();

        string [] parts = size.Split ('_');

        if (parts.Length == 0)
            return BadRequest ();
        if (int.TryParse (parts [0], out int sizeInt) == false)
            return BadRequest ();
        if (int.TryParse (file, out int typeID) == false)
            return BadRequest ();
        
        // finally call the other endpoint
        return Types (sizeInt, typeID);
    }
}