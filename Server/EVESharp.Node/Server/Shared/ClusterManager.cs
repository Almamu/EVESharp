using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using EVESharp.Common.Logging;
using EVESharp.EVE.Packets;
using EVESharp.Node.Server.Shared.Transports;
using Serilog;

namespace EVESharp.Node.Server.Shared;

public class ClusterManager
{
    public  IMachoNet        MachoNet         { get; }
    public  ILogger          Log              { get; }
    public  TransportManager TransportManager { get; }
    public  HttpClient       HttpClient       { get; }
    private DateTime         IntervalStart    { get; set; }
    private int              IntervalDuration { get; set; }
    private Thread           mClusterTimer;
    /// <summary>
    /// The event used when a timed event has to happen cluster-wide
    /// </summary>
    public EventHandler OnClusterTimer { get; set; }

    public ClusterManager (IMachoNet machoNet, TransportManager transportManager, HttpClient httpClient, ILogger logger)
    {
        MachoNet           = machoNet;
        Log                = logger;
        TransportManager   = transportManager;
        HttpClient         = httpClient;
        this.mClusterTimer = new Thread (this.RunClusterTimerThread);

        if (machoNet.Mode == RunMode.Single)
        {
            IntervalStart    = DateTime.Now;
            IntervalDuration = 5;
            
            // start up the thread
            this.mClusterTimer.Start ();            
        }
    }

    /// <summary>
    /// Register the given IMachoNet instance with the orchestrator and updates it's information
    /// </summary>
    public async void RegisterNode ()
    {
        // register ourselves with the orchestrator and get our node id AND address
        HttpContent content = new FormUrlEncodedContent (
            new Dictionary <string, string>
            {
                {"port", MachoNet.Port.ToString ()},
                {
                    "role", MachoNet.Mode switch
                    {
                        RunMode.Proxy  => "proxy",
                        RunMode.Server => "server",
                        _              => "single"
                    }
                }
            }
        );
        HttpResponseMessage response = await HttpClient.PostAsync ($"{MachoNet.OrchestratorURL}/Nodes/register", content);

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode ();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync ();

        JsonObject result = JsonSerializer.Deserialize <JsonObject> (inputStream);

        MachoNet.Address = result ["address"].ToString ();
        MachoNet.NodeID  = (long) result ["nodeId"];
        IntervalStart    = DateTime.FromFileTimeUtc((long) result ["startupTime"]);
        IntervalDuration = (int) result ["timeInterval"];

        MachoNet.Log.Information ($"Orchestrator assigned node id {MachoNet.NodeID} with address {MachoNet.Address}");
    
        // TODO: PROPERLY SELECT THIS AS THIS PREVENTS MULTIPLE PROXIES ACTUALLY WORKING AT ONCE
        if (MachoNet.Mode == RunMode.Proxy)
            // start the cluster timer thread
            this.mClusterTimer.Start ();
    }

    /// <summary>
    /// Sends a heartbeat to the orchestrator agent to signal our node being up and running healthily
    /// </summary>
    public async void PerformHeartbeat ()
    {
        MachoNet.Log.Debug ("Sending heartbeat to orchestration agent");
        // register ourselves with the orchestrator and get our node id AND address
        HttpContent content = new FormUrlEncodedContent (
            new Dictionary <string, string>
            {
                {"address", MachoNet.Address},
                {"load", "0.0"}
            }
        );
        await HttpClient.PostAsync ($"{MachoNet.OrchestratorURL}/Nodes/heartbeat", content);
    }

    /// <summary>
    /// Ensures the identification req is actually legitimate
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public async Task <bool> ValidateIdentificationReq (IdentificationReq req)
    {
        // register ourselves with the orchestrator and get our node id AND address
        HttpResponseMessage response = await HttpClient.GetAsync ($"{MachoNet.OrchestratorURL}/Nodes/{req.Address}");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode ();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync ();

        JsonObject result = JsonSerializer.Deserialize <JsonObject> (inputStream);

        // validate the data we've received back
        return await Task.FromResult (req.NodeID == (long) result ["nodeID"] && req.Mode == result ["role"].ToString ());
    }

    /// <summary>
    /// Opens a connection to the given proxy
    /// </summary>
    /// <param name="nodeID">The nodeID of the proxy to connect to</param>
    public async Task <MachoTransport> OpenNodeConnection (long nodeID)
    {
        // check if there's a connection already and return that one instead
        if (TransportManager.NodeTransports.TryGetValue (nodeID, out MachoNodeTransport nodeTransport))
            return nodeTransport;

        MachoNet.Log.Information ($"Looking up NodeID {nodeID}...");
        HttpResponseMessage response = await HttpClient.GetAsync ($"{MachoNet.OrchestratorURL}/Nodes/node/{nodeID}");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode ();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync ();

        JsonObject result = JsonSerializer.Deserialize <JsonObject> (inputStream);

        // get address and port
        string ip   = result ["ip"].ToString ();
        ushort port = (ushort) result ["port"];
        string role = result ["role"].ToString ();

        MachoNet.Log.Information ($"Found {role} with NodeID {nodeID} on address {ip}, opening connection...");

        // finally open a connection and register it in the transport list
        MachoUnauthenticatedTransport transport =
            new MachoUnauthenticatedTransport (MachoNet, HttpClient, Log.ForContext <MachoUnauthenticatedTransport> (result ["ip"].ToString ()));
        // open a connection
        transport.Connect (ip, port);
        // send an identification req to start the authentication flow
        transport.Socket.Send (
            new IdentificationReq
            {
                Address = MachoNet.Address,
                NodeID  = MachoNet.NodeID,
                Mode = MachoNet.Mode switch
                {
                    RunMode.Proxy  => "proxy",
                    RunMode.Server => "server",
                    _              => "single"
                }
            }
        );

        return transport;
    }

    public async void EstablishConnectionWithProxies ()
    {
        HttpResponseMessage response = await HttpClient.GetAsync ($"{MachoNet.OrchestratorURL}/Nodes/proxies");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode ();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync ();

        JsonArray result = JsonSerializer.Deserialize <JsonArray> (inputStream);

        foreach (JsonObject proxy in result)
        {
            long nodeID = (long) proxy ["nodeID"];

            await this.OpenNodeConnection (nodeID);
        }
    }

    private void RunClusterTimerThread ()
    {
        while (true)
        {
            TimeSpan minutesTimespan = TimeSpan.FromMinutes (IntervalDuration);
            long     startTime       = IntervalStart.Ticks;
            long     currentTime     = DateTime.Now.Ticks;
            long     diff            = currentTime - startTime;
            long     timesCluster    = diff / minutesTimespan.Ticks;
            long     startNextTime   = startTime + (timesCluster + 1) * minutesTimespan.Ticks;

            // wait for the required time until the next timer fires
            Thread.Sleep (TimeSpan.FromTicks (startNextTime - currentTime));

            Log.Debug ("Running periodic cluster timed events");
            
            this.OnClusterTimer?.Invoke (this, null);
        }
    }
}