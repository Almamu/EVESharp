using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoUnauthenticatedTransport : MachoTransport
{
    public MachoUnauthenticatedTransport(MachoServerTransport transport, Channel channel) :
        base(transport, new EVEClientSocket(channel), channel)
    {
        this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchange);
        this.Socket.SetExceptionHandler(HandleException);
    }

    public MachoUnauthenticatedTransport(MachoServerTransport transport, EVEClientSocket socket, Logger logger) : base(transport, socket, logger)
    {
        // send low level version exchange to start authorization chain
        this.SendLowLevelVersionExchange();
        this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchange);
        this.Socket.SetExceptionHandler(HandleException);
    }

    public void Connect(string ip, ushort port)
    {
        // connect
        this.Socket.Connect(ip, port);
        // send the LowLevelVersionExchange to validate versions
        this.SendLowLevelVersionExchange();
    }

    private void ReceiveLowLevelVersionExchange(PyDataType ar)
    {
        // store the remote address in the session
        this.Session.Address = this.Socket.GetRemoteAddress();
        
        // depending on the type of data we're receiving, this has to be treated differently
        this.HandleLowLevelVersionExchange(ar);
    }

    private void HandleIdentificationReq(IdentificationReq req)
    {
        // validate the identification req against the orchestration manager first
        Task<bool> validation = this.ValidateIdentificationReq(req);
        // wait for the task to be completed
        validation.Wait();

        // if the connection could not be validated do nothing else
        if (validation.Result == false)
        {
            Log.Fatal("IdentificationReq and Orchestrator do not say the same thing, aborting connection");
            this.AbortConnection();
            return;
        }
        
        // store the NodeID in the session
        this.Session.NodeID = req.NodeID;

        // send our identification response back and store the connection as whatever it is
        // so we can start receiving data
        switch (req.Mode)
        {
            case "proxy":
                // resolve the connection to a proxy transport
                this.Server.ResolveProxyTransport(this);
                break;
            case "server":
                // resolve the connection to a node transport
                this.Server.ResolveNodeTransport(this);
                break;
        }
        
        // send our identification res so the destination knows what to do
        this.Socket.Send(
            new IdentificationRsp()
            {
                Accepted = true,
                NodeID = this.Server.MachoNet.Container.NodeID,
                Mode = this.Server.MachoNet.Configuration.MachoNet.Mode switch
                {
                    MachoNetMode.Proxy => "proxy",
                    MachoNetMode.Server => "server"
                }
            }
        );
    }

    private void HandleIdentificationRsp(IdentificationRsp rsp)
    {
        // store the NodeID in the session
        this.Session.NodeID = rsp.NodeID;
        
        switch (rsp.Mode)
        {
            case "proxy":
                // resolve the connection to a proxy transport
                this.Server.ResolveProxyTransport(this);
                break;
            case "server":
                // resolve the connection to a node transport
                this.Server.ResolveNodeTransport(this);
                break;
        }
    }
    
    private void HandleIdentificationFlow(PyPacket packet)
    {
        switch (packet.Type)
        {
            case PyPacket.PacketType.IDENTIFICATION_REQ:
                this.HandleIdentificationReq(packet);
                break;
            case PyPacket.PacketType.IDENTIFICATION_RSP:
                this.HandleIdentificationRsp(packet);
                break;
        }
    }

    private async Task<bool> ValidateIdentificationReq(IdentificationReq req)
    {
        // register ourselves with the orchestrator and get our node id AND address
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync($"{this.Server.MachoNet.Configuration.Cluster.OrchestatorURL}/Nodes/{req.Address}");

        // make sure we have a proper answer
        response.EnsureSuccessStatusCode();
        // read the json and extract the required information
        Stream inputStream = await response.Content.ReadAsStreamAsync();

        JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

        // validate the data we've received back
        return await Task.FromResult(req.NodeID == (long) result["nodeID"] && req.Mode == result["role"].ToString());
    }
    
    private void HandleLowLevelVersionExchange(LowLevelVersionExchange ex)
    {
        Log.Debug("Handling low level version exchange");
            
        // assign the new packet handler to wait for commands again
        this.Socket.SetReceiveCallback(ReceiveCommandCallback);
    }

    private void HandleException(Exception ex)
    {
        Log.Error("Exception detected: ");

        do
        {
            Log.Error(ex.Message);
            Log.Error(ex.StackTrace);
        } while ((ex = ex.InnerException) != null);
    }

    protected void SendLowLevelVersionExchange()
    {
        Log.Debug("Sending LowLevelVersionExchange...");

        LowLevelVersionExchange data = new LowLevelVersionExchange
        {
            Codename = Game.CODENAME,
            Birthday = Game.BIRTHDAY,
            Build = Game.BUILD,
            MachoVersion = Game.MACHO_VERSION,
            Version = Game.VERSION,
            UserCount = this.Server.ClientTransports.Count,
            Region = Game.REGION
        };

        this.Socket.Send(data);
    }
    
    private void ReceiveCommandCallback(PyDataType packet)
    {
        if (PyPacket.IsPyPacket(packet) == true)
        {
            this.HandleIdentificationFlow(packet);
            return;
        }

        ClientCommand command = packet;

        if (command.Command == "QC")
        {
            Log.Debug("Received QueueCheck command");
            // send player position on the queue
            this.Socket.Send(new PyInteger(this.Server.MachoNet.LoginQueue.Count()));
            // send low level version exchange required
            this.SendLowLevelVersionExchange();
            // wait for a new low level version exchange again
            this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchange);
        }
        else if (command.Command == "VK")
        {
            Log.Debug("Received VipKey command");
            // next is the placebo challenge
            this.Socket.SetReceiveCallback(ReceiveCryptoRequestCallback);
        }
        else
        {
            throw new Exception("Received unknown data!");
        }
    }

    private void ReceiveCryptoRequestCallback(PyDataType packet)
    {
        PlaceboRequest request = packet;

        if (request.Command != "placebo")
            throw new InvalidDataException($"Unknown command {request.Command}, expected 'placebo'");

        if (request.Arguments.Length > 0)
            Log.Warning("Received PlaceboRequest with extra arguments, this is not supported");

        Log.Debug("Received correct Crypto request");
        // answer the client with a correct crypto challenge
        this.Socket.Send(new PyString("OK CC"));
        // next is the first login attempt
        this.Socket.SetReceiveCallback(ReceiveAuthenticationRequestCallback);
    }

    private void ReceiveAuthenticationRequestCallback(PyDataType packet)
    {
        AuthenticationReq request = packet;

        if (request.user_password is null)
        {
            Log.Trace("Rejected by server; requesting plain password");
            // request the user a plain password
            this.Socket.Send(new PyInteger(1)); // 1 => plain, 2 => hashed
            return;
        }

        // TODO: DYNAMICALLY FETCH THIS SO WE SUPPORT TRANSLATIONS
        if (request.user_languageid != "EN" && request.user_languageid != "RU" && request.user_languageid != "DE")
            // default to english language
            this.Session.LanguageID = "EN";
        else
            // set languageid in the session to the one requested as we have translations for that one
            this.Session.LanguageID = request.user_languageid;

        // add the user to the authentication queue
        this.Server.MachoNet.LoginQueue.Enqueue(this, request);
    }

    private void ReceiveLoginResultResponse(PyDataType packet)
    {
        PyTuple data = packet as PyTuple;

        if (data.Count != 3)
            throw new Exception($"Expected tuple to have 3 items but got {data.Count}");

        // Handshake sent when we are mostly in
        HandshakeAck ack = new HandshakeAck
        {
            LiveUpdates = this.Server.MachoNet.GeneralDB.FetchLiveUpdates(),
            JIT = this.Session.LanguageID,
            UserID = this.Session.UserID,
            MaxSessionTime = null,
            UserType = AccountType.USER,
            Role = this.Session.Role,
            Address = this.Session.Address,
            InDetention = null,
            ClientHashes = new PyList(),
            UserClientID = this.Session.UserID
        };
        
        // send the response first
        this.Socket.Send(ack);
        // move the connection to the authenticated user list
        this.Server.ResolveClientTransport(this);
        // send the initial session state
        this.Server.MachoNet.SessionManager.InitializeSession(this.Session);
    }

    public void SendLoginNotification(LoginStatus loginStatus, int accountID, ulong role)
    {
        if (loginStatus == LoginStatus.Success)
        {
            // We should check for a exact number of nodes here when we have the needed infrastructure
            if (true)
            {
                AuthenticationRsp rsp = new AuthenticationRsp();

                // String "None" marshaled
                byte[] func_marshaled_code = new byte[] {0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65};

                rsp.serverChallenge = "";
                rsp.func_marshaled_code = func_marshaled_code;
                rsp.verification = false;
                rsp.cluster_usercount = this.Server.ClientTransports.Count;
                rsp.proxy_nodeid = this.Server.MachoNet.Container.NodeID;
                rsp.user_logonqueueposition = 1;
                rsp.challenge_responsehash = "55087";

                rsp.macho_version = Game.MACHO_VERSION;
                rsp.boot_version = Game.VERSION;
                rsp.boot_build = Game.BUILD;
                rsp.boot_codename = Game.CODENAME;
                rsp.boot_region = Game.REGION;

                // setup session
                this.Session.UserType = AccountType.USER;
                this.Session.UserID = accountID;
                this.Session.Role = role;
                // send the login response
                this.Socket.Send(rsp);
                // set second to last packet handler
                this.Socket.SetReceiveCallback(ReceiveLoginResultResponse);
            }
            else
            {
                // TODO: IMPLEMENT CLUSTER STARTUP
                // Pretty funny, "AutClusterStarting" maybe they mean "AuthClusterStarting"
                this.Socket.Send(new GPSTransportClosed("AutClusterStarting"));

                Log.Trace("Rejected by server; cluster is starting");

                this.AbortConnection();
            }
        }
        else if (loginStatus == LoginStatus.Failed)
        {
            this.Socket.Send(new GPSTransportClosed("LoginAuthFailed"));
            this.AbortConnection();
        }
    }
    
}