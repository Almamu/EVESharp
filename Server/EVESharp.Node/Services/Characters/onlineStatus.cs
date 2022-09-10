using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Database;
using EVESharp.Types;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class onlineStatus : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;
    private         ChatDB      ChatDB      { get; }
    private         OldCharacterDB CharacterDB { get; }
    private         IItems      Items       { get; }

    public onlineStatus (ChatDB chatDB, OldCharacterDB characterDB, IItems items)
    {
        ChatDB      = chatDB;
        CharacterDB = characterDB;
        this.Items  = items;
    }

    public PyDataType GetInitialState (CallInformation call)
    {
        // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK
        return ChatDB.GetAddressBookMembers (call.Session.CharacterID);
    }

    public PyDataType GetOnlineStatus (CallInformation call, PyInteger characterID)
    {
        // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK?
        return CharacterDB.IsOnline (characterID);
    }
}