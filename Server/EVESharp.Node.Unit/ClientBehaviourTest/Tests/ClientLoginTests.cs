using System;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Common.Network.Sockets;
using EVESharp.EVE;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Services;
using EVESharp.EVE.Unit.Packets;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Server.Single.Messages;
using EVESharp.Node.Sessions;
using EVESharp.Node.Unit.Utils;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using HarmonyLib;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using TestExtensions;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

[TestFixture]
public class ClientLoginTests
{
    private readonly Harmony                              mHarmony              = new Harmony ("ClientLoginTests");
    private readonly Mock <IDatabaseConnection>           mDatabase             = new Mock <IDatabaseConnection> ();
    private readonly Mock <Authentication>                mAuthenticationMock   = new Mock <Authentication> ();
    private readonly Mock <General>                       mGeneralMock          = new Mock <General> ();
    private readonly Mock <Common.Configuration.MachoNet> mMachoNetMock         = new Mock <Common.Configuration.MachoNet> ();
    private readonly Mock <Cluster>                       mClusterMock          = new Mock <Cluster> ();
    private readonly Mock <IRemoteServiceManager>         mRemoteServiceManager = new Mock <IRemoteServiceManager> ();
    private readonly Mock <INotificationSender>           mNotificationSender   = new Mock <INotificationSender> ();
    private readonly Mock <IItems>                        mItems                = new Mock <IItems> ();
    private readonly Mock <ISolarSystems>                 mSolarSystems         = new Mock <ISolarSystems> ();

    private IMachoNet                              mMachoNet;
    private SynchronousProcessor <LoginQueueEntry> mLoginProcessor;
    private SessionManager                         mSessionManager;
    private TestEveClientSocket                    mSocket;
    private bool                                   mQueueCheckDone = false;
    private bool                                   mTestDone       = false;

    [SetUp]
    public void SetUp ()
    {
        this.mHarmony.Setup (typeof (HarmonyPatches));
        
        // setup authentication configuration
        this.mAuthenticationMock.SetupGet (x => x.Autoaccount).Returns (false);
        this.mAuthenticationMock.SetupGet (x => x.MessageType).Returns (AuthenticationMessageType.NoMessage);
        this.mAuthenticationMock.SetupGet (x => x.Role).Returns ((long) Roles.ROLE_PLAYER);
        
        // setup machonet configuration
        this.mMachoNetMock.SetupGet (x => x.Mode).Returns (MachoNetMode.Single);
        this.mMachoNetMock.SetupGet (x => x.Port).Returns (26000);
        
        // setup cluster configuration
        this.mClusterMock.SetupGet (x => x.OrchestatorURL).Returns ("INVALID URL!");
        
        // setup general configuration
        this.mGeneralMock.SetupGet (x => x.Authentication).Returns (this.mAuthenticationMock.Object);
        this.mGeneralMock.SetupGet (x => x.MachoNet).Returns (this.mMachoNetMock.Object);
        this.mGeneralMock.SetupGet (x => x.Cluster).Returns (this.mClusterMock.Object);

        this.mLoginProcessor = new SynchronousProcessor <LoginQueueEntry> (
            new LoginQueue (this.mDatabase.Object, this.mAuthenticationMock.Object, Logger.None)
        );
    }

    [TearDown]
    public void TearDown ()
    {
        this.mHarmony.UnpatchAll ();
    }

    [Test]
    public void SingleMachoNetStartupTest ()
    {
        // transport manager
        TransportManager transportManager = new TransportManager (new HttpClient (), Logger.None);
        // macho net
        this.mMachoNet = new MachoNet (
            this.mDatabase.Object, transportManager, this.mLoginProcessor, this.mGeneralMock.Object, Logger.None
        );
        // session manager
        this.mSessionManager = new SessionManager (transportManager, this.mMachoNet);
        // message processor
        SynchronousProcessor <MachoMessage> processor = new SynchronousProcessor <MachoMessage> (
            new MessageQueue (
                this.mMachoNet,
                Logger.None,
                null,
                null,
                Mock.Of <IRemoteServiceManager> (),
                new PacketCallHelper (this.mMachoNet),
                this.mNotificationSender.Object,
                this.mItems.Object,
                this.mSolarSystems.Object,
                this.mSessionManager
            )
        );

        this.mMachoNet.Initialize ();
        this.mMachoNet.MessageProcessor = processor;
        this.mSocket = (transportManager.ServerTransport as TestMachoServerTransport).SimulateNewConnection ();

        this.mSocket.DataSent += this.ExpectLowLevelVersionExchange;
        // process the login queue now
        this.mLoginProcessor.ProcessNextMessage ();
        Assert.True (this.mTestDone);
    }

    private void ExpectLowLevelVersionExchange (PyDataType data)
    {
        LowLevelVersionExchangeTests.AssertLowLevelVersionExchange (data, 0);
        // send the LowLevelVersionExchange back to start the authentication process so it continues the chain
        this.mSocket.SimulateDataReceived (data);
        // if no queue check was done, send one
        if (this.mQueueCheckDone == false)
        {
            this.mSocket.DataSent -= this.ExpectLowLevelVersionExchange;
            this.mSocket.DataSent += this.ExpectQueueCheckResponse;
            this.mQueueCheckDone  =  true;
            this.mSocket.SimulateDataReceived (new ClientCommand ("QC"));
        }
        else
        {
            // update the packet handler
            this.mSocket.DataSent -= this.ExpectLowLevelVersionExchange;
            this.mSocket.DataSent += this.ExpectOKCC;
            // send vipkey command and placebo request
            this.mSocket.SimulateDataReceived (new ClientCommand ("VK"));
            this.mSocket.SimulateDataReceived (new PlaceboRequest ("placebo", new PyDictionary()));
        }
    }

    private void ExpectQueueCheckResponse (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectQueueCheckResponse;
        this.mSocket.DataSent += this.ExpectLowLevelVersionExchange;
        // we should be the only one in the queue
        PyAssert.Integer (data, 0);
    }

    private void ExpectOKCC (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectOKCC;
        this.mSocket.DataSent += this.ExpectPlainPasswordRequest;
        
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
    }

    private void ExpectPlainPasswordRequest (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectPlainPasswordRequest;
        this.mSocket.DataSent += this.ExpectLoginResult;
        
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
    }

    private void ExpectLoginResult (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectLoginResult;
        this.mSocket.DataSent += this.ExpectHandshakeAcknowledge;
        
        AuthenticationRsp rsp = data;

        Assert.AreEqual (this.mMachoNet.NodeID, rsp.proxy_nodeid);
        Assert.AreEqual (1,                     rsp.user_logonqueueposition);
        Assert.AreEqual ("55087",               rsp.challenge_responsehash);
        
        // send login result, this one is not really parsed by the server, so no extra stuff to set
        this.mSocket.SimulateDataReceived (new PyTuple (3));
    }

    private void ExpectHandshakeAcknowledge (PyDataType data)
    {
        this.mSocket.DataSent -= this.ExpectHandshakeAcknowledge;
        this.mSocket.DataSent += this.ExpectSessionInitialState;
        HandshakeAck ack = data;
        
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
        
        Assert.AreEqual (PyPacket.PacketType.SESSIONINITIALSTATENOTIFICATION, packet.Type);
        Assert.AreEqual ("macho.SessionInitialStateNotification",             packet.TypeString);

        this.mTestDone = true;
    }
}