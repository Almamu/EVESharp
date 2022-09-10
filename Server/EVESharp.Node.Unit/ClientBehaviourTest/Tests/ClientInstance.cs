using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Network;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Types.Network;
using EVESharp.EVE.Unit.Packets;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class ClientInstance
{
    /// <summary>
    /// The socket acting as a client
    /// </summary>
    private readonly TestEveClientSocket mSocket;

    /// <summary>
    /// The macho net handling this socket
    /// </summary>
    private readonly IMachoNet mMachoNet;

    /// <summary>
    /// Indicates if the QC command was already issued or not
    /// </summary>
    private bool SentQueueCheck { get;             set; } = false;
    /// <summary>
    /// Indicates if the VK command was already issued or not
    /// </summary>
    private bool SentVipKey                 { get; set; } = false;
    /// <summary>
    /// Indicates if the AuthenticationReq was already issued or not
    /// </summary>
    private bool SentAuthenticationReq      { get; set; } = false;
    /// <summary>
    /// Indicates if the plain AuthenticationReq was already issued or not
    /// </summary>
    private bool SentPlainAuthenticationReq { get; set; } = false;
    /// <summary>
    /// Indicates if the login result response was already issued or not
    /// </summary>
    private bool SentLoginResultResponse    { get; set; } = false;
    /// <summary>
    /// Indicates if the first low level version exchange was received
    /// </summary>
    private bool ReceivedFirstLowLevelVersionExchange { get; set; }
    /// <summary>
    /// Indicates if the second low level version exchange was received
    /// </summary>
    private bool ReceivedSecondLowLevelVersionExchange { get; set; }
    /// <summary>
    /// Indicates if the QueueCheck response was received
    /// </summary>
    private bool ReceivedQueueCheckResponse { get; set; }
    /// <summary>
    /// Indicates if the OK CC packet was received
    /// </summary>
    private bool ReceivedOKCC { get; set; }
    /// <summary>
    /// Indicates if the plain password request was received
    /// </summary>
    private bool ReceivedPlainPasswordRequest { get; set; }
    /// <summary>
    /// Indicates if the login result was received
    /// </summary>
    private bool ReceivedLoginResult { get; set; }
    /// <summary>
    /// Indicates if the HandshakAck was received
    /// </summary>
    private bool ReceivedHandshakeAck { get; set; }
    /// <summary>
    /// Indicates if the session initial state was received
    /// </summary>
    private bool ReceivedSessionInitialState { get; set; }
    /// <summary>
    /// Indicates if the login process was completed properly
    /// </summary>
    private bool LoginProcessDone { get; set; }

    public ClientInstance (TestEveClientSocket socket, IMachoNet machoNet)
    {
        this.mMachoNet        =  machoNet;
        this.mSocket          =  socket;
        this.mSocket.DataSent += this.ExpectLowLevelVersionExchange;
    }
    
    private void ExpectLowLevelVersionExchange (PyDataType data)
    {
        LowLevelVersionExchangeTests.AssertLowLevelVersionExchange (data, 0);
        // send the LowLevelVersionExchange back to start the authentication process so it continues the chain
        this.mSocket.SimulateDataReceived (data);
        // if no queue check was done, send one
        if (this.ReceivedQueueCheckResponse == false)
        {
            this.ReceivedFirstLowLevelVersionExchange = true;
                
            this.mSocket.DataSent -= this.ExpectLowLevelVersionExchange;
            this.mSocket.DataSent += this.ExpectQueueCheckResponse;
            this.mSocket.SimulateDataReceived (new ClientCommand ("QC"));
            this.SentQueueCheck = true;
        }
        else
        {
            this.ReceivedSecondLowLevelVersionExchange = true;
            // update the packet handler
            this.mSocket.DataSent -= this.ExpectLowLevelVersionExchange;
            this.mSocket.DataSent += this.ExpectOKCC;
            // send vipkey command and placebo request
            this.mSocket.SimulateDataReceived (new ClientCommand ("VK"));
            this.mSocket.SimulateDataReceived (new PlaceboRequest ("placebo", new PyDictionary()));
            this.SentVipKey = true;
        }
    }

    private void ExpectQueueCheckResponse (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectQueueCheckResponse;
        this.mSocket.DataSent += this.ExpectLowLevelVersionExchange;
        // we should be the only one in the queue
        PyAssert.Integer (data, 0);
        this.ReceivedQueueCheckResponse = true;
    }

    private void ExpectOKCC (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectOKCC;
        this.mSocket.DataSent += this.ExpectPlainPasswordRequest;

        this.ReceivedOKCC = true;
        
        PyAssert.String (data, "OK CC");
        
        // send the authentication request now and expect the plain password request
        AuthenticationReq req = new AuthenticationReq
        {
            boot_build         = EVE.Data.Version.BUILD,
            boot_codename      = EVE.Data.Version.CODENAME,
            boot_region        = EVE.Data.Version.REGION,
            boot_version       = EVE.Data.Version.VERSION,
            macho_version      = EVE.Data.Version.MACHO_VERSION,
            user_affiliateid   = 0,
            user_languageid    = "EN",
            user_name          = "Almamu",
            user_password_hash = "HASH"
        };

        this.mSocket.SimulateDataReceived (req);
        this.SentAuthenticationReq = true;
    }

    private void ExpectPlainPasswordRequest (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectPlainPasswordRequest;
        this.mSocket.DataSent += this.ExpectLoginResult;

        this.ReceivedPlainPasswordRequest = true;
        
        PyAssert.Integer (data, 1);
        
        // send the actual AuthenticationReq
        AuthenticationReq req = new AuthenticationReq
        {
            boot_build       = EVE.Data.Version.BUILD,
            boot_codename    = EVE.Data.Version.CODENAME,
            boot_region      = EVE.Data.Version.REGION,
            boot_version     = EVE.Data.Version.VERSION,
            macho_version    = EVE.Data.Version.MACHO_VERSION,
            user_affiliateid = 0,
            user_languageid  = "EN",
            user_name        = "Almamu",
            user_password    = "Password"
        };

        this.mSocket.SimulateDataReceived (req);

        this.SentPlainAuthenticationReq = true;
    }

    private void ExpectLoginResult (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectLoginResult;
        this.mSocket.DataSent += this.ExpectHandshakeAcknowledge;

        this.ReceivedLoginResult = true;
        
        AuthenticationRsp rsp = data;

        Assert.AreEqual (this.mMachoNet.NodeID, rsp.proxy_nodeid);
        Assert.AreEqual (1,                     rsp.user_logonqueueposition);
        Assert.AreEqual ("55087",               rsp.challenge_responsehash);
        
        // send login result, this one is not really parsed by the server, so no extra stuff to set
        this.mSocket.SimulateDataReceived (new PyTuple (3));

        this.SentLoginResultResponse = true;
    }

    private void ExpectHandshakeAcknowledge (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectHandshakeAcknowledge;
        this.mSocket.DataSent += this.ExpectSessionInitialState;
        HandshakeAck ack = data;

        this.ReceivedHandshakeAck = true;
        
        // ensure some data is right
        PyAssert.List (ack.LiveUpdates,  0);
        PyAssert.List (ack.ClientHashes, 0);
        Assert.AreEqual ("FakeSocket", ack.Address);
        Assert.AreEqual ("EN",         ack.JIT);
        Assert.AreEqual (1,            ack.UserID);
        Assert.AreEqual (1,            ack.UserClientID);
        Assert.AreEqual (
            (ulong) Roles.ROLE_PLAYER | (ulong) Roles.ROLE_LOGIN | (ulong) Roles.ROLE_ADMIN | (ulong) Roles.ROLE_QA | (ulong) Roles.ROLE_SPAWN |
            (ulong) Roles.ROLE_GML | (ulong) Roles.ROLE_GDL | (ulong) Roles.ROLE_GDH | (ulong) Roles.ROLE_HOSTING | (ulong) Roles.ROLE_PROGRAMMER,
            ack.Role
        );

        Assert.AreEqual (AccountType.USER, ack.UserType);
        Assert.IsNull (ack.MaxSessionTime);
        Assert.IsNull (ack.InDetention);
    }

    private void ExpectSessionInitialState (PyDataType data)
    {
        PyPacket packet = data;

        this.ReceivedSessionInitialState = true;
        
        Assert.AreEqual (PyPacket.PacketType.SESSIONINITIALSTATENOTIFICATION, packet.Type);
        Assert.AreEqual ("macho.SessionInitialStateNotification",             packet.TypeString);

        this.LoginProcessDone = true;
    }

    /// <summary>
    /// Performs verifications for this instance
    /// </summary>
    public void Verify ()
    {
        Assert.True (this.SentQueueCheck);
        Assert.True (this.SentVipKey);
        Assert.True (this.SentAuthenticationReq);
        Assert.True (this.SentPlainAuthenticationReq);
        Assert.True (this.SentLoginResultResponse);
        Assert.True (this.ReceivedFirstLowLevelVersionExchange);
        Assert.True (this.ReceivedSecondLowLevelVersionExchange);
        Assert.True (this.ReceivedQueueCheckResponse);
        Assert.True (this.ReceivedOKCC);
        Assert.True (this.ReceivedPlainPasswordRequest);
        Assert.True (this.ReceivedLoginResult);
        Assert.True (this.ReceivedHandshakeAck);
        Assert.True (this.ReceivedSessionInitialState);
        Assert.True (this.LoginProcessDone);
    }
}