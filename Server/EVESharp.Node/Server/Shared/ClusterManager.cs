using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EVESharp.Common.Logging;
using EVESharp.EVE.Packets;
using EVESharp.Node.Configuration;
using EVESharp.Node.Network;
using Serilog;

namespace EVESharp.Node.Server.Shared;

public class ClusterManager
{
    public IMachoNet MachoNet { get; }
    public ILogger Log { get; }
    public TransportManager TransportManager { get; }
    public HttpClient HttpClient { get; }
    
    public ClusterManager(IMachoNet machoNet, TransportManager transportManager, HttpClient httpClient, ILogger logger)
    {
        this.MachoNet = machoNet;
        this.Log = logger;
        this.TransportManager = transportManager;
        this.HttpClient = httpClient;
    }
    
    /// <summary>
    /// Register the given IMachoNet instance with the orchestrator and updates it's information
    /// </summary>
    public async void RegisterNode()
    {
        // register ourselves with the orchestrator and get our node id AND address
        HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
            {"port", this.MachoNet.Port.ToString()},
            {"role", this.MachoNet.Mode switch
            {
                RunMode.Proxy => "proxy",
                RunMode.Server => "server"
            }}
        });
        HttpResponseMessage response = await this.HttpClient.PostAsync($"{this.MachoNet.OrchestratorURL}/Nodes/register",  content);

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync();

        JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

        this.MachoNet.Address = result["address"].ToString();
        this.MachoNet.NodeID = (long) result["nodeId"];
            
        this.MachoNet.Log.Information($"Orchestrator assigned node id {this.MachoNet.NodeID} with address {this.MachoNet.Address}");
    }

    /// <summary>
    /// Sends a heartbeat to the orchestrator agent to signal our node being up and running healthily
    /// </summary>
    public async void PerformHeartbeat()
    {
        this.MachoNet.Log.Debug("Sending heartbeat to orchestration agent");
        // register ourselves with the orchestrator and get our node id AND address
        HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
            {"address", this.MachoNet.Address},
            {"load", "0.0"}
        });
        await this.HttpClient.PostAsync($"{this.MachoNet.OrchestratorURL}/Nodes/heartbeat",  content);
    }

    /// <summary>
    /// Ensures the identification req is actually legitimate
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public async Task<bool> ValidateIdentificationReq(IdentificationReq req)
    {
        // register ourselves with the orchestrator and get our node id AND address
        HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.MachoNet.OrchestratorURL}/Nodes/{req.Address}");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync();

        JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

        // validate the data we've received back
        return await Task.FromResult(req.NodeID == (long) result["nodeID"] && req.Mode == result["role"].ToString());

    }

    /// <summary>
    /// Opens a connection to the given proxy
    /// </summary>
    /// <param name="nodeID">The nodeID of the proxy to connect to</param>
    public async Task<MachoTransport> OpenNodeConnection(long nodeID)
    {
        // check if there's a connection already and return that one instead
        if (this.TransportManager.NodeTransports.TryGetValue(nodeID, out MachoNodeTransport nodeTransport) == true)
            return nodeTransport;
        
        this.MachoNet.Log.Information($"Looking up NodeID {nodeID}...");
        HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.MachoNet.OrchestratorURL}/Nodes/node/{nodeID}");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync();

        JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

        // get address and port
        string ip = result["ip"].ToString();
        ushort port = (ushort) result["port"];
        string role = result["role"].ToString();
        
        this.MachoNet.Log.Information($"Found {role} with NodeID {nodeID} on address {ip}, opening connection...");
        
        // finally open a connection and register it in the transport list
        MachoUnauthenticatedTransport transport =
            new MachoUnauthenticatedTransport(this.MachoNet, this.HttpClient, Log.ForContext<MachoUnauthenticatedTransport>(result["ip"].ToString()));
        // open a connection
        transport.Connect(ip, port);
        // send an identification req to start the authentication flow
        transport.Socket.Send(
            new IdentificationReq()
            {
                Address = this.MachoNet.Address,
                NodeID = this.MachoNet.NodeID,
                Mode = this.MachoNet.Mode switch
                {
                    RunMode.Proxy => "proxy",
                    RunMode.Server => "server"
                }
            }
        );
        
        return transport;
    }
    
    public async void EstablishConnectionWithProxies()
    {
        HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.MachoNet.OrchestratorURL}/Nodes/proxies");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync();

        JsonArray result = JsonSerializer.Deserialize<JsonArray>(inputStream);

        foreach (JsonObject proxy in result)
        {
            long nodeID = (long) proxy["nodeID"];

            await this.OpenNodeConnection(nodeID);
        }
    }
}