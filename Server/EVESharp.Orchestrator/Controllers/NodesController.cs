using EVESharp.Database;
using EVESharp.Orchestrator.Models;
using EVESharp.Orchestrator.Providers;
using EVESharp.Orchestrator.Repositories;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;
using Microsoft.AspNetCore.Mvc;

namespace EVESharp.Orchestrator.Controllers;

[ApiController]
[Route ("[controller]")]
public class NodesController : ControllerBase
{
    private IDatabaseConnection      DB                  { get; }
    private ILogger<NodesController> Logger              { get; }
    private IConfiguration           Configuration       { get; }
    private IStartupInfoProvider     StartupInfoProvider { get; }
    private IClusterRepository       ClusterRepository   { get; }

    public NodesController (
        IDatabaseConnection  db,
        ILogger<NodesController>      logger,
        IStartupInfoProvider startupInfoProvider,
        IConfiguration       configuration,
        IClusterRepository   clusterRepository)
    {
        this.DB                  = db;
        this.Logger              = logger;
        this.Configuration       = configuration;
        this.StartupInfoProvider = startupInfoProvider;
        this.ClusterRepository   = clusterRepository;
    }

    [HttpGet (Name = "GetNodeList")]
    [Produces ("application/json")]
    public ActionResult <IEnumerable <Node>> GetNodeList ()
    {
        return this.ClusterRepository.FindNodes ();
    }

    [HttpGet ("{address}")]
    [Produces ("application/json")]
    public ActionResult <Node> GetNodeByAddress (string address)
    {
        try
        {
            return this.ClusterRepository.FindByAddress (address);
        }
        catch (InvalidDataException e)
        {
            return this.NotFound (e.Message);
        }
    }

    [HttpGet ("node/{nodeId}")]
    [Produces ("application/json")]
    public ActionResult <Node> GetNodeInformation (int nodeId)
    {
        try
        {
            return this.ClusterRepository.FindById (nodeId);
        }
        catch (InvalidDataException e)
        {
            return this.NotFound (e.Message);
        }
    }

    [HttpGet ("proxies")]
    [Produces ("application/json")]
    public ActionResult <List <Node>> GetProxies ()
    {
        return this.ClusterRepository.FindProxyNodes ();
    }

    [HttpGet ("servers")]
    [Produces ("application/json")]
    public ActionResult <List <Node>> GetServers ()
    {
        return this.ClusterRepository.FindServerNodes ();
    }

    [HttpPost ("register")]
    [Produces ("application/json")]
    public ActionResult <object> RegisterNewNode ([FromForm] ushort port, [FromForm] string role)
    {
        if (role != "proxy" && role != "server")
            return this.BadRequest ($"Unknown node role... {role}");

        Node newNode = this.ClusterRepository.RegisterNode (
            this.Request.HttpContext.Connection.RemoteIpAddress?.ToString ()!,
            port,
            role
        );
        
        this.Logger.LogInformation ("Registered a new node with address {newNode.Address}, coming from IP {newNode.IP}", newNode.Address, newNode.IP);

        return new
        {
            NodeId       = newNode.NodeID,
            Address      = newNode.Address,
            TimeInterval = int.Parse (this.Configuration.GetSection ("Cluster") ["TimedEventsInterval"]),
            StartupTime  = this.StartupInfoProvider.Time.ToFileTimeUtc ()
        };
    }

    [HttpPost ("heartbeat")]
    public void DoHeartbeat ([FromForm] string address, [FromForm] float load)
    {
        this.Logger.LogInformation ("Received heartbeat from {address} with load {load}", address, load);

        this.ClusterRepository.Hearbeat (address, load);
    }

    [HttpGet ("next")]
    [Produces ("application/json")]
    public ActionResult <long> GetNextNode ()
    {
        try
        {
            Node node = this.ClusterRepository.GetLeastLoadedNode ();
            
            this.Logger.LogInformation ("Returned node ({node.NodeID}) with lowest load ({node.Load})", node.NodeID, node.Load);

            return node.NodeID;
        }
        catch (InvalidDataException e)
        {
            return this.NotFound (e.Message);
        }
    }
}