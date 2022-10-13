using EVESharp.API.Models;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EVESharp.API.Controllers;

[ApiController]
[Route("/server/")]
public class ServerController : ControllerBase
{
    private readonly IDatabase DB;
    
    public ServerController (IDatabase db)
    {
        DB = db;
    }
    
    [Route("ServerStatus.xml.aspx")]
    [HttpGet]
    public ActionResult <EVEApiModel <ServerStatusModel>> ServerStatus ()
    {
        return new EVEApiModel <ServerStatusModel>
        {
            Result = new ServerStatusModel
            {
                OnlinePlayers = DB.ChrGetOnlineCount (),
                ServerOpen    = true // TODO: PROPERLY IMPLEMENT SERVER STATUS CHECKS
            }
        };
    }
}