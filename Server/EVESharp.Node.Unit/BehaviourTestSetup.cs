using EVESharp.Node.Unit.Utils;
using HarmonyLib;
using NUnit.Framework;

namespace EVESharp.Node.Unit;

[SetUpFixture]
public class BehaviourTestSetup
{
    private readonly Harmony mHarmony = new Harmony ("GlobalDatabasePatches");
    
    [OneTimeSetUp]
    public void GlobalSetup ()
    {
        this.mHarmony.Setup (typeof (MemoryDatabase.Patches.Accounts));
        this.mHarmony.Setup (typeof (MemoryDatabase.Patches.Cluster));
        this.mHarmony.Setup (typeof (MemoryDatabase.Patches.Wallets));
        this.mHarmony.Setup (typeof (MemoryDatabase.Patches.Item));
        this.mHarmony.Setup (typeof (MemoryDatabase.Patches.Settings));
    }

    [OneTimeTearDown]
    public void GlobalTeardown ()
    {
        this.mHarmony.UnpatchAll ();
    }
}