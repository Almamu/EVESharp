using EVESharp.EVE;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.jumpCloneSvc;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Clones;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class jumpCloneSvc : ClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    private ItemDB              ItemDB              { get; }
    private MarketDB            MarketDB            { get; }
    private ItemFactory         ItemFactory         { get; }
    private TypeManager         TypeManager         => ItemFactory.TypeManager;
    private SystemManager       SystemManager       { get; }
    private NotificationManager NotificationManager { get; }
    private WalletManager       WalletManager       { get; }

    public jumpCloneSvc (
        ItemDB        itemDB,        MarketDB      marketDB,      ItemFactory         itemFactory,
        SystemManager systemManager, WalletManager walletManager, NotificationManager notificationManager, BoundServiceManager manager
    ) : base (manager)
    {
        ItemDB              = itemDB;
        MarketDB            = marketDB;
        ItemFactory         = itemFactory;
        SystemManager       = systemManager;
        WalletManager       = walletManager;
        NotificationManager = notificationManager;
    }

    protected jumpCloneSvc (
        int           locationID,    ItemDB              itemDB,  MarketDB      marketDB,      ItemFactory         itemFactory,
        SystemManager systemManager, BoundServiceManager manager, WalletManager walletManager, NotificationManager notificationManager, Session session
    ) : base (manager, session, locationID)
    {
        ItemDB              = itemDB;
        MarketDB            = marketDB;
        ItemFactory         = itemFactory;
        SystemManager       = systemManager;
        WalletManager       = walletManager;
        NotificationManager = notificationManager;
    }

    /// <summary>
    /// Sends a OnJumpCloneCacheInvalidated notification to the specified character so the clone window
    /// is reloaded
    /// </summary>
    /// <param name="characterID">The character to notify</param>
    public void OnCloneUpdate (int characterID)
    {
        NotificationManager.NotifyCharacter (characterID, new OnJumpCloneCacheInvalidated ());
    }

    public PyDataType GetCloneState (CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["clones"]       = ItemDB.GetClonesForCharacter (callerCharacterID, (int) character.ActiveCloneID),
                ["implants"]     = ItemDB.GetImplantsForCharacterClones (callerCharacterID),
                ["timeLastJump"] = character.TimeLastJump
            }
        );
    }

    public PyDataType DestroyInstalledClone (PyInteger jumpCloneID, CallInformation call)
    {
        // if the clone is not loaded the clone cannot be removed, players can only remove clones from where they're at
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (ItemFactory.TryGetItem (jumpCloneID, out ItemEntity clone) == false)
            throw new JumpCantDestroyNonLocalClone ();
        if (clone.LocationID != call.Session.LocationID)
            throw new JumpCantDestroyNonLocalClone ();
        if (clone.OwnerID != callerCharacterID)
            throw new MktNotOwner ();

        // finally destroy the clone, this also destroys all the implants in it
        ItemFactory.DestroyItem (clone);

        // let the client know that the clones were updated
        this.OnCloneUpdate (callerCharacterID);

        return null;
    }

    public PyDataType GetShipCloneState (CallInformation call)
    {
        return ItemDB.GetClonesInShipForCharacter (call.Session.EnsureCharacterIsSelected ());
    }

    public PyDataType CloneJump (PyInteger locationID, PyBool unknown, CallInformation call)
    {
        // TODO: IMPLEMENT THIS CALL PROPERLY, INVOLVES SESSION CHANGES
        // TODO: AND SEND PROPER NOTIFICATION AFTER A JUMP CLONE OnJumpCloneTransitionCompleted
        return null;
    }

    public PyInteger GetPriceForClone (CallInformation call)
    {
        // TODO: CALCULATE THIS ON POS, AS THIS VALUE IS STATIC OTHERWISE

        // seems to be hardcoded for npc's stations
        return 100000;
    }

    public PyDataType InstallCloneInStation (CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        int stationID         = call.Session.EnsureCharacterIsInStation ();

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        // check the maximum number of clones the character has assigned
        long maximumClonesAvailable = character.GetSkillLevel (Types.InfomorphPsychology);

        // the skill is not trained
        if (maximumClonesAvailable == 0)
            throw new JumpCharStoringMaxClonesNone ();

        // get list of clones (excluding the medical clone)
        Rowset clones = ItemDB.GetClonesForCharacter (character.ID, (int) character.ActiveCloneID);

        // ensure we don't have more than the allowed clones
        if (clones.Rows.Count >= maximumClonesAvailable)
            throw new JumpCharStoringMaxClones (clones.Rows.Count, maximumClonesAvailable);

        // ensure that the character has enough money
        int cost = this.GetPriceForClone (call);

        // get character's station
        Station station = ItemFactory.GetStaticStation (stationID);

        using Wallet wallet = WalletManager.AcquireWallet (character.ID, WalletKeys.MAIN_WALLET);
        {
            wallet.EnsureEnoughBalance (cost);
            wallet.CreateJournalRecord (MarketReference.JumpCloneInstallationFee, null, station.ID, -cost, $"Installed clone at {station.Name}");
        }

        // create an alpha clone
        Type cloneType = TypeManager [Types.CloneGradeAlpha];

        // create a new clone on the itemDB
        Clone clone = ItemFactory.CreateClone (cloneType, station, character);

        // finally create the jump clone and invalidate caches
        this.OnCloneUpdate (callerCharacterID);

        // persist the character information
        character.Persist ();

        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        int solarSystemID = 0;

        if (parameters.ExtraValue == (int) Groups.SolarSystem)
            solarSystemID = ItemFactory.GetStaticSolarSystem (parameters.ObjectID).ID;
        else if (parameters.ExtraValue == (int) Groups.Station)
            solarSystemID = ItemFactory.GetStaticStation (parameters.ObjectID).SolarSystemID;
        else
            throw new CustomError ("Unknown item's groupID");

        if (SystemManager.SolarSystemBelongsToUs (solarSystemID))
            return BoundServiceManager.MachoNet.NodeID;

        return SystemManager.GetNodeSolarSystemBelongsTo (solarSystemID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        // ensure this node will take care of the instance
        long nodeID = this.MachoResolveObject (bindParams, call);

        if (nodeID != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new jumpCloneSvc (
            bindParams.ObjectID, ItemDB, MarketDB, ItemFactory, SystemManager, BoundServiceManager, WalletManager, NotificationManager,
            call.Session
        );
    }
}