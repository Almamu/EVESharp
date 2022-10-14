using System.Data.Common;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Orchestrator.Models;

namespace EVESharp.Orchestrator.Repositories;

public class ClusterRepository : IClusterRepository
{
    private IDatabase DB { get; }

    public ClusterRepository (IDatabase db, IConfiguration configuration, ILogger<ClusterRepository> logger)
    {
        this.DB = db;
        
        // check if things have to be cleared
        if (bool.Parse (configuration.GetSection ("Cluster") ["ResetOnStartup"]) != true)
            return;

        logger.LogInformation ("Cleaning up cluster information");
            
        db.InvClearNodeAssociation ();
        db.CluResetClientAddresses ();
        db.CluCleanup ();
        db.ChrClearLoginStatus ();
    }
    
    public List <Node> FindNodes ()
    {
        List <Node> result = new List <Node> ();

        using (DbDataReader reader = this.DB.Select ("SELECT id, address, ip, port, role, lastHeartBeat FROM cluster;"))
        {
            while (reader.Read ())
            {
                Node node = new Node
                {
                    NodeID        = reader.GetInt32 (0),
                    Address       = reader.GetString (1),
                    IP            = reader.GetString (2),
                    Port          = reader.GetInt16 (3),
                    Role          = reader.GetString (4),
                    LastHeartBeat = reader.GetInt64 (5)
                };

                result.Add (node);
            }
        }

        return result;
    }

    public Node FindByAddress (string address)
    {
        DbDataReader reader = this.DB.Select (
            "SELECT id, address, ip, port, role, lastHeartBeat FROM cluster WHERE address = @address;",
            new Dictionary <string, object> ()
            {
                {"@address", address}
            }
        );
        
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Cannot find a node with the given address");

            return new Node
            {
                NodeID        = reader.GetInt32 (0),
                Address       = reader.GetString (1),
                IP            = reader.GetString (2),
                Port          = reader.GetInt32 (3),
                Role          = reader.GetString (4),
                LastHeartBeat = reader.GetInt64 (5)
            };
        }
    }

    public Node FindById (int nodeId)
    {
        DbDataReader reader = this.DB.Select (
            "SELECT id, address, ip, port, role, lastHeartBeat FROM cluster WHERE id = @nodeID;",
            new Dictionary <string, object> ()
            {
                {"@nodeID", nodeId}
            }
        );
        
        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Cannot find a node with the given id");

            return new Node
            {
                NodeID        = reader.GetInt32 (0),
                Address       = reader.GetString (1),
                IP            = reader.GetString (2),
                Port          = reader.GetInt16 (3),
                Role          = reader.GetString (4),
                LastHeartBeat = reader.GetInt64 (5)
            };
        }
    }

    private List <Node> FindByMode (string mode)
    {
        DbDataReader reader = this.DB.Select (
            "SELECT id, address, ip, port, role, lastHeartBeat FROM cluster WHERE role = @mode;",
            new Dictionary <string, object> ()
            {
                {"@mode", "proxy"}
            }
        );
        
        using (reader)
        {
            List <Node> result = new List <Node> ();

            while (reader.Read ())
                result.Add (
                    new Node
                    {
                        NodeID        = reader.GetInt32 (0),
                        Address       = reader.GetString (1),
                        IP            = reader.GetString (2),
                        Port          = reader.GetInt16 (3),
                        Role          = reader.GetString (4),
                        LastHeartBeat = reader.GetInt64 (5)
                    }
                );

            return result;
        }
    }

    public List <Node> FindProxyNodes ()
    {
        return this.FindByMode ("proxy");
    }

    public List <Node> FindServerNodes ()
    {
        return this.FindByMode ("server");
    }

    public Node RegisterNode (string ip, ushort port, string role)
    {
        if (role != "proxy" && role != "server")
            throw new InvalidDataException ("Node role can only be \"proxy\" or \"server\"");

        long currentTime = DateTime.UtcNow.ToFileTimeUtc ();
        Guid address     = Guid.NewGuid ();
        
        ulong nodeId = this.DB.Insert (
            "INSERT INTO cluster(id, ip, address, port, role, lastHeartBeat)VALUES(NULL, @ip, @address, @port, @role, @lastHeartBeat);",
            new Dictionary <string, object> ()
            {
                {"@ip", ip},
                {"@address", address.ToString ()},
                {"@port", port},
                {"@role", role},
                {"@lastHeartBeat", currentTime}
            }
        );
        
        return new Node
        {
            NodeID = (int) nodeId,
            Address = address.ToString (),
            IP   = ip,
            Port = port,
            Role = role,
            LastHeartBeat = currentTime,
        };
    }

    public Node GetLeastLoadedNode ()
    {
        DbDataReader reader = this.DB.Select (
            "SELECT id, address, ip, port, role, lastHeartBeat, `load` FROM cluster WHERE lastHeartBeat > @lastHeartBeat AND role LIKE @role ORDER BY `load` DESC LIMIT 1",
            new Dictionary <string, object> ()
            {
                {"@lastHeartBeat", DateTime.Now.AddSeconds (-90).ToFileTimeUtc ()},
                {"@role", "server"}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Cannot find any active node to load");

            return new Node
            {
                NodeID        = reader.GetInt32 (0),
                Address       = reader.GetString (1),
                IP            = reader.GetString (2),
                Port          = reader.GetInt16 (3),
                Role          = reader.GetString (4),
                LastHeartBeat = reader.GetInt64 (5),
                Load          = reader.GetDouble (6)
            };
        }
    }

    public void Hearbeat (string address, double load)
    {
        this.DB.Query (
            "UPDATE cluster SET lastHeartBeat = @lastHeartBeat, `load` = @load WHERE address LIKE @address;",
            new Dictionary <string, object> ()
            {
                {"@lastHeartBeat", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@load", load},
                {"@address", address}
            }
        );
    }
}