using System;
using System.IO;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.Node.Accounts;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoUnauthenticatedTransport : MachoTransport
{
    public MachoUnauthenticatedTransport(MachoServerTransport transport, EVEClientSocket socket, Logger logger) : base(transport, socket, logger)
    {
        // store the remote address first
        this.Session.Address = socket.GetRemoteAddress();
        // send low level version exchange to start authorization chain
        this.SendLowLevelVersionExchange();
        this.Socket.SetReceiveCallback(ReceiveFirstMessageCallback);
        this.Socket.SetExceptionHandler(HandleException);
    }

    private void ReceiveFirstMessageCallback(PyDataType ar)
    {
        try
        {
            // depending on the type of data we're receiving, this has to be treated differently
            if (ar is PyObjectData)
            {
                this.HandleIdentificationReq(ar);
            }
            else
            {
                this.HandleLowLevelVersionExchange(ar);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Exception caught on IdentificationReq/LowLevelVersionExchange: {e.Message}");
            throw;
        }
    }

    private void HandleIdentificationReq(IdentificationReq req)
    {
        
        Log.Error("Cannot handle identification req yet!");
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

    private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
    {
        try
        {
            this.HandleLowLevelVersionExchange(ar);
        }
        catch (Exception e)
        {
            Log.Error($"Exception caught on LowLevelVersionExchange: {e.Message}");
            throw;
        }
    }

    private void ReceiveCommandCallback(PyDataType packet)
    {
        ClientCommand command = packet;

        if (command.Command == "QC")
        {
            Log.Debug("Received QueueCheck command");
            // send player position on the queue
            this.Socket.Send(new PyInteger(this.Server.MachoNet.LoginQueue.Count()));
            // send low level version exchange required
            this.SendLowLevelVersionExchange();
            // wait for a new low level version exchange again
            this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
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
            // We should check for a exact number of nodes here when we have the needed infraestructure
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