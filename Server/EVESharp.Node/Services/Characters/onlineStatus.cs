using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class onlineStatus : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;
    private         ChatDB      ChatDB      { get; }
    private         CharacterDB CharacterDB { get; }
    private         IItems Items { get; }

    public onlineStatus (ChatDB chatDB, CharacterDB characterDB, IItems items)
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