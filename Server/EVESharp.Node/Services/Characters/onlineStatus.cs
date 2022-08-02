using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class onlineStatus : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;
    private         ChatDB      ChatDB      { get; }
    private         CharacterDB CharacterDB { get; }
    private         ItemFactory ItemFactory { get; }

    public onlineStatus (ChatDB chatDB, CharacterDB characterDB, ItemFactory itemFactory)
    {
        ChatDB      = chatDB;
        CharacterDB = characterDB;
        ItemFactory = itemFactory;
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