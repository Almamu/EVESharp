using EVESharp.Orchestator.Models;
using EVESharp.Orchestator.Providers;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace EVESharp.Orchestator.Controllers;

[ApiController]
[Route ("[controller]")]
public class NodesController : ControllerBase
{
    private Database                  DB                  { get; }
    private ILogger <NodesController> Logger              { get; }
    private IConfiguration            Configuration       { get; }
    private IStartupInfoProvider      StartupInfoProvider { get; }

    public NodesController (Database db, ILogger <NodesController> logger, IStartupInfoProvider startupInfoProvider, IConfiguration configuration)
    {
        DB                  = db;
        Logger              = logger;
        Configuration       = configuration;
        StartupInfoProvider = startupInfoProvider;
    }

    [HttpGet (Name = "GetNodeList")]
    [Produces ("application/json")]
    public ActionResult <IEnumerable <Node>> GetNodeList ()
    {
        List <Node> result = new List <Node> ();

        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("SELECT id, ip, port, role, lastHeartBeat FROM cluster;", connection);

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                while (reader.Read ())
                {
                    Node node = new Node
                    {
                        NodeID        = reader.GetInt32 (0),
                        IP            = reader.GetString (1),
                        Port          = reader.GetInt16 (2),
                        Role          = reader.GetString (3),
                        LastHeartBeat = reader.GetInt64 (4)
                    };

                    result.Add (node);
                }
            }
        }

        return result;
    }

    [HttpGet ("{address}")]
    [Produces ("application/json")]
    public ActionResult <Node> GetNodeByAddress (string address)
    {
        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE address = @address;", connection);

            cmd.Parameters.AddWithValue ("@address", address);
            cmd.Prepare ();

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                if (reader.Read () == false)
                    return this.NotFound ();

                return new Node
                {
                    NodeID        = reader.GetInt32 (0),
                    IP            = reader.GetString (1),
                    Port          = reader.GetInt16 (2),
                    Role          = reader.GetString (3),
                    LastHeartBeat = reader.GetInt64 (4)
                };
            }
        }
    }

    [HttpGet ("node/{nodeID}")]
    [Produces ("application/json")]
    public ActionResult <Node> GetNodeInformation (int nodeID)
    {
        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE id = @nodeID;", connection);

            cmd.Parameters.AddWithValue ("@nodeID", nodeID);
            cmd.Prepare ();

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                if (reader.Read () == false)
                    return this.NotFound ();

                return new Node
                {
                    NodeID        = reader.GetInt32 (0),
                    IP            = reader.GetString (1),
                    Port          = reader.GetInt16 (2),
                    Role          = reader.GetString (3),
                    LastHeartBeat = reader.GetInt64 (4)
                };
            }
        }
    }

    [HttpGet ("proxies")]
    [Produces ("application/json")]
    public ActionResult <List <Node>> GetProxies ()
    {
        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE role = @mode;", connection);

            cmd.Parameters.AddWithValue ("@mode", "proxy");
            cmd.Prepare ();

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                List <Node> result = new List <Node> ();

                while (reader.Read ())
                    result.Add (
                        new Node
                        {
                            NodeID        = reader.GetInt32 (0),
                            IP            = reader.GetString (1),
                            Port          = reader.GetInt16 (2),
                            Role          = reader.GetString (3),
                            LastHeartBeat = reader.GetInt64 (4)
                        }
                    );

                return result;
            }
        }
    }

    [HttpGet ("servers")]
    [Produces ("application/json")]
    public ActionResult <List <Node>> GetServers ()
    {
        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE role = @mode;", connection);

            cmd.Parameters.AddWithValue ("@mode", "server");
            cmd.Prepare ();

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                List <Node> result = new List <Node> ();

                while (reader.Read ())
                    result.Add (
                        new Node
                        {
                            NodeID        = reader.GetInt32 (0),
                            IP            = reader.GetString (1),
                            Port          = reader.GetInt16 (2),
                            Role          = reader.GetString (3),
                            LastHeartBeat = reader.GetInt64 (4)
                        }
                    );

                return result;
            }
        }
    }

    [HttpPost ("register")]
    [Produces ("application/json")]
    public ActionResult <object> RegisterNewNode ([FromForm] ushort port, [FromForm] string role)
    {
        if (role != "proxy" && role != "server")
            return this.BadRequest ($"Unknown node role... {role}");

        long   currentTime = DateTime.UtcNow.ToFileTimeUtc ();
        string ip          = Request.HttpContext.Connection.RemoteIpAddress?.ToString ()!;
        Guid   address     = Guid.NewGuid ();

        Logger.LogInformation ("Registering a new node with address {address}, coming from IP {ip}", address, ip);

        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand (
                "INSERT INTO cluster(id, ip, address, port, role, lastHeartBeat)VALUES(NULL, @ip, @address, @port, @role, @lastHeartBeat);", connection
            );

            cmd.Parameters.AddWithValue ("@ip",            ip);
            cmd.Parameters.AddWithValue ("@address",       address.ToString ());
            cmd.Parameters.AddWithValue ("@port",          port);
            cmd.Parameters.AddWithValue ("@role",          role);
            cmd.Parameters.AddWithValue ("@lastHeartBeat", currentTime);

            cmd.ExecuteNonQuery ();
            long nodeId = cmd.LastInsertedId;

            // return the data required for the node to understand what it is
            return new
            {
                NodeId  = nodeId,
                Address = address.ToString (),
                TimeInterval = int.Parse (Configuration.GetSection ("Cluster") ["TimedEventsInterval"]),
                StartupTime = StartupInfoProvider.Time.ToFileTimeUtc ()
            };
        }
    }

    [HttpPost ("heartbeat")]
    public void DoHeartbeat ([FromForm] string address, [FromForm] float load)
    {
        Logger.LogInformation ("Received heartbeat from {address} with load {load}", address, load);

        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand ("UPDATE cluster SET lastHeartBeat = @lastHeartBeat, `load` = @load WHERE address LIKE @address;", connection);

            cmd.Parameters.AddWithValue ("@lastHeartBeat", DateTime.UtcNow.ToFileTimeUtc ());
            cmd.Parameters.AddWithValue ("@load",          load);
            cmd.Parameters.AddWithValue ("@address",       address);

            cmd.Prepare ();
            cmd.ExecuteNonQuery ();
        }
    }

    [HttpGet ("next")]
    [Produces ("application/json")]
    public ActionResult <long> GetNextNode ()
    {
        using (MySqlConnection connection = DB.Get ())
        {
            MySqlCommand cmd = new MySqlCommand (
                "SELECT id, `load` FROM cluster WHERE lastHeartBeat > @lastHeartBeat AND role LIKE @role ORDER BY `load` DESC LIMIT 1;", connection
            );

            cmd.Parameters.AddWithValue ("@lastHeartBeat", DateTime.Now.AddSeconds (-90).ToFileTimeUtc ());
            cmd.Parameters.AddWithValue ("@role",          "server");
            cmd.Prepare ();

            using (MySqlDataReader reader = cmd.ExecuteReader ())
            {
                // this should be something more appropriate, but bad request should be more than enough
                if (reader.Read () == false)
                    return this.BadRequest ();

                Logger.LogInformation ("Returned node ({nodeID}) with lowest load ({load})", reader.GetInt32 (0), reader.GetDouble (1));

                return reader.GetInt64 (0);
            }
        }
    }
}