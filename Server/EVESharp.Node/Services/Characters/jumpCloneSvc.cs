﻿using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Clones;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;
using ItemDB = EVESharp.Database.Old.ItemDB;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class jumpCloneSvc : ClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    private ItemDB              ItemDB        { get; }
    private MarketDB            MarketDB      { get; }
    private IItems              Items         { get; }
    private ITypes              Types         => this.Items.Types;
    private ISolarSystems       SolarSystems  { get; }
    private INotificationSender Notifications { get; }
    private IWallets            Wallets       { get; }
    private IDatabase Database      { get; }

    public jumpCloneSvc
    (
        ItemDB        itemDB,       MarketDB marketDB, IItems              items,
        ISolarSystems solarSystems, IWallets wallets,  INotificationSender notificationSender, IBoundServiceManager manager, IDatabase database
    ) : base (manager)
    {
        ItemDB            = itemDB;
        MarketDB          = marketDB;
        this.Items        = items;
        this.SolarSystems = solarSystems;
        this.Wallets      = wallets;
        Notifications     = notificationSender;
        Database          = database;
    }

    protected jumpCloneSvc
    (
        int           locationID,   ItemDB               itemDB, MarketDB marketDB, IItems items,
        ISolarSystems solarSystems, IBoundServiceManager manager, IWallets wallets,  INotificationSender notificationSender, Session session
    ) : base (manager, session, locationID)
    {
        ItemDB            = itemDB;
        MarketDB          = marketDB;
        this.Items        = items;
        this.SolarSystems = solarSystems;
        this.Wallets      = wallets;
        Notifications     = notificationSender;
    }

    /// <summary>
    /// Sends a OnJumpCloneCacheInvalidated notification to the specified character so the clone window
    /// is reloaded
    /// </summary>
    /// <param name="characterID">The character to notify</param>
    public void OnCloneUpdate (int characterID)
    {
        Notifications.NotifyCharacter (characterID, new OnJumpCloneCacheInvalidated ());
    }

    public PyDataType GetCloneState (ServiceCall call)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["clones"]       = ItemDB.GetClonesForCharacter (callerCharacterID, (int) character.ActiveCloneID),
                ["implants"]     = ItemDB.GetImplantsForCharacterClones (callerCharacterID),
                ["timeLastJump"] = character.TimeLastJump
            }
        );
    }

    public PyDataType DestroyInstalledClone (ServiceCall call, PyInteger jumpCloneID)
    {
        // if the clone is not loaded the clone cannot be removed, players can only remove clones from where they're at
        int callerCharacterID = call.Session.CharacterID;

        if (this.Items.TryGetItem (jumpCloneID, out ItemEntity clone) == false)
            throw new JumpCantDestroyNonLocalClone ();

        if (clone.LocationID != call.Session.LocationID)
            throw new JumpCantDestroyNonLocalClone ();

        if (clone.OwnerID != callerCharacterID)
            throw new MktNotOwner ();

        // finally destroy the clone, this also destroys all the implants in it
        this.Items.DestroyItem (clone);

        // let the client know that the clones were updated
        this.OnCloneUpdate (callerCharacterID);

        return null;
    }

    public PyDataType GetShipCloneState (ServiceCall call)
    {
        return ItemDB.GetClonesInShipForCharacter (call.Session.CharacterID);
    }

    public PyDataType CloneJump (ServiceCall call, PyInteger locationID, PyBool unknown)
    {
        // TODO: IMPLEMENT THIS CALL PROPERLY, INVOLVES SESSION CHANGES
        // TODO: AND SEND PROPER NOTIFICATION AFTER A JUMP CLONE OnJumpCloneTransitionCompleted
        return null;
    }

    public PyInteger GetPriceForClone (ServiceCall call)
    {
        // TODO: CALCULATE THIS ON POS, AS THIS VALUE IS STATIC OTHERWISE

        // seems to be hardcoded for npc's stations
        return 100000;
    }

    [MustBeInStation]
    public PyDataType InstallCloneInStation (ServiceCall call)
    {
        int callerCharacterID = call.Session.CharacterID;
        int stationID         = call.Session.StationID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        // check the maximum number of clones the character has assigned
        long maximumClonesAvailable = character.GetSkillLevel (TypeID.InfomorphPsychology);

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
        Station station = this.Items.GetStaticStation (stationID);

        using IWallet wallet = this.Wallets.AcquireWallet (character.ID, WalletKeys.MAIN);

        {
            wallet.EnsureEnoughBalance (cost);
            wallet.CreateJournalRecord (MarketReference.JumpCloneInstallationFee, null, station.ID, -cost, $"Installed clone at {station.Name}");
        }

        // create an alpha clone
        Type cloneType = this.Types [TypeID.CloneGradeAlpha];

        // create a new clone on the itemDB
        Clone clone = this.Items.CreateClone (cloneType, station, character);

        // finally create the jump clone and invalidate caches
        this.OnCloneUpdate (callerCharacterID);

        // persist the character information
        character.Persist ();

        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return parameters.ExtraValue switch
        {
            (int) GroupID.SolarSystem => Database.CluResolveAddress ("solarsystem", parameters.ObjectID),
            (int) GroupID.Station     => Database.CluResolveAddress ("station",     parameters.ObjectID),
            _                         => throw new CustomError ("Unknown item's groupID")
        };
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new jumpCloneSvc (
            bindParams.ObjectID, ItemDB, MarketDB, this.Items, this.SolarSystems, BoundServiceManager, this.Wallets, Notifications,
            call.Session
        );
    }
}