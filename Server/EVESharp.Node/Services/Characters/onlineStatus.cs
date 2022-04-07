using System;
using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class onlineStatus : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;
    private         ChatDB      ChatDB      { get; }
    private         CharacterDB CharacterDB { get; }
    private         ItemFactory ItemFactory { get; }
        
    public onlineStatus(ChatDB chatDB, CharacterDB characterDB, ItemFactory itemFactory)
    {
        this.ChatDB      = chatDB;
        this.CharacterDB = characterDB;
        this.ItemFactory = itemFactory;
    }

    public PyDataType GetInitialState(CallInformation call)
    {
        // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK
        return this.ChatDB.GetAddressBookMembers(call.Session.EnsureCharacterIsSelected());
    }

    public PyDataType GetOnlineStatus(PyInteger characterID, CallInformation call)
    {
        // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK?
        return this.CharacterDB.IsOnline(characterID);
    }
}