using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.Orchestator.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace EVESharp.Orchestator.Controllers;

[ApiController]
[Route("[controller]")]
public class NodesController : ControllerBase
{
    private Database DB { get; init; }
    private ILogger<NodesController> Logger { get; init; }
    
    public NodesController(Database db, ILogger<NodesController> logger)
    {
        this.DB = db;
        this.Logger = logger;
    }
    
    [HttpGet(Name = "GetNodeList")]
    public IEnumerable<Node> GetNodeList()
    {
        List<Node> result = new List<Node>();
        
        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id, ip, port, role, lastHeartBeat FROM cluster;", connection);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read() == true)
                {
                    Node node = new Node
                    {
                        NodeID = reader.GetInt32(0),
                        IP = reader.GetString(1),
                        Port = reader.GetInt16(2),
                        Role = reader.GetString(3),
                        LastHeartBeat = reader.GetInt64(4),
                    };

                    result.Add(node);
                }
            }
        }

        return result;
    }
    
    [HttpGet("{address}")]
    public ActionResult<Node> GetNodeByAddress(string address)
    {
        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE address = @address;", connection);

            cmd.Parameters.AddWithValue("@address", address);
            cmd.Prepare();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read() == false)
                    return NotFound();
                
                return new Node
                {
                    NodeID = reader.GetInt32(0),
                    IP = reader.GetString(1),
                    Port = reader.GetInt16(2),
                    Role = reader.GetString (3),
                    LastHeartBeat = reader.GetInt64(4)
                };
            }
        }
    }
    
    [HttpGet("node/{nodeID}")]
    public ActionResult<Node> GetNodeInformation(int nodeID)
    {
        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE id = @nodeID;", connection);

            cmd.Parameters.AddWithValue("@nodeID", nodeID);
            cmd.Prepare();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read() == false)
                    return NotFound();
                
                return new Node
                {
                    NodeID = reader.GetInt32(0),
                    IP = reader.GetString(1),
                    Port = reader.GetInt16(2),
                    Role = reader.GetString (3),
                    LastHeartBeat = reader.GetInt64(4)
                };
            }
        }
    }

    [HttpGet("proxies")]
    public ActionResult<List<Node>> GetProxies()
    {
        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE role = @mode;", connection);

            cmd.Parameters.AddWithValue("@mode", "proxy");
            cmd.Prepare();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                List<Node> result = new List<Node>();

                while (reader.Read() == true)
                {
                    result.Add(new Node
                    {
                        NodeID = reader.GetInt32(0),
                        IP = reader.GetString(1),
                        Port = reader.GetInt16(2),
                        Role = reader.GetString (3),
                        LastHeartBeat = reader.GetInt64(4)
                    });
                }

                return result;
            }
        }
    }

    [HttpGet("servers")]
    public ActionResult<List<Node>> GetServers()
    {
        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id, ip, port, role, lastHeartBeat FROM cluster WHERE role = @mode;", connection);

            cmd.Parameters.AddWithValue("@mode", "server");
            cmd.Prepare();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                List<Node> result = new List<Node>();

                while (reader.Read() == true)
                {
                    result.Add(new Node
                    {
                        NodeID = reader.GetInt32(0),
                        IP = reader.GetString(1),
                        Port = reader.GetInt16(2),
                        Role = reader.GetString (3),
                        LastHeartBeat = reader.GetInt64(4)
                    });
                }

                return result;
            }
        }
    }

    [HttpPost("register")]
    public object RegisterNewNode([FromForm]ushort port, [FromForm]string role)
    {
        if (role != "proxy" && role != "server")
            return this.BadRequest ($"Unknown node role... {role}");
        
        long currentTime = DateTime.UtcNow.ToFileTimeUtc();
        string ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString()!;
        Guid address = Guid.NewGuid();
        
        Logger.LogInformation("Registering a new node with address {address}, coming from IP {ip}", address, ip);

        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("INSERT INTO cluster(id, ip, address, port, role, lastHeartBeat)VALUES(NULL, @ip, @address, @port, @role, @lastHeartBeat);", connection);

            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@address", address.ToString());
            cmd.Parameters.AddWithValue("@port", port);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@lastHeartBeat", currentTime);

            cmd.ExecuteNonQuery();
            long nodeId = cmd.LastInsertedId;
            
            // return the data required for the node to understand what it is
            return new
            {
                NodeId = nodeId,
                Address = address.ToString()
            };
        }
    }

    [HttpPost("heartbeat")]
    public void DoHeartbeat([FromBody] string address)
    {
        Logger.LogInformation("Received heartbeat from {address}", address);

        using (MySqlConnection connection = this.DB.Get())
        {
            MySqlCommand cmd = new MySqlCommand("UPDATE cluster SET lastHeartBeat = @lastHeartBeat WHERE address = @address;", connection);

            cmd.Parameters.AddWithValue("@lastHeartBeat", DateTime.UtcNow.ToFileTimeUtc());
            cmd.Parameters.AddWithValue("@address", address);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }
}