using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Configuration;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Network;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Unit.Utils;
using EVESharp.PythonTypes.Types.Database;
using HarmonyLib;
using Moq;
using NUnit.Framework;
using Serilog;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

[TestFixture]
public class ClientLoginTests
{
    private readonly Harmony                                mHarmony  = new Harmony ("ClientLoginTests");
    private          Mock <IDatabaseConnection>             mDatabase = new Mock <IDatabaseConnection> ();
    private          SynchronousProcessor <LoginQueueEntry> mLoginProcessor;
    private          Mock <Authentication>                  mAuthenticationMock = new Mock <Authentication> ();
    private          Mock <General>                         mGeneralMock        = new Mock <General> ();
    private          Mock <Common.Configuration.MachoNet>   mMachoNetMock       = new Mock <Common.Configuration.MachoNet> ();
    private          Mock <Cluster>                         mClusterMock        = new Mock <Cluster> ();

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
            new LoginQueue (this.mDatabase.Object, this.mAuthenticationMock.Object, Log.Logger.ForContext <LoginQueue> ())
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
        TransportManager transportManager = new TransportManager (new HttpClient (), Log.Logger.ForContext <TransportManager> ());
        IMachoNet        machoNet = new MachoNet (
            this.mDatabase.Object, transportManager, this.mLoginProcessor, this.mGeneralMock.Object, Log.Logger.ForContext <MachoNet> ()
        );
        
        machoNet.Initialize ();
    }
}