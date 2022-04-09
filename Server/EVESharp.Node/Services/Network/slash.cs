using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE;
using EVESharp.EVE.Client.Exceptions.slash;
using EVESharp.EVE.Market;
using EVESharp.EVE.Services;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Notifications.Client.Skills;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using Categories = EVESharp.EVE.StaticData.Inventory.Categories;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.Node.Services.Network;

public class slash : Service
{
    private readonly Dictionary <string, Action <string [], CallInformation>> mCommands =
        new Dictionary <string, Action <string [], CallInformation>> ();
    public override AccessLevel                 AccessLevel         => AccessLevel.None;
    private         TypeManager                 TypeManager         => ItemFactory.TypeManager;
    private         ItemFactory                 ItemFactory         { get; }
    private         ILogger                     Log                 { get; }
    private         CharacterDB                 CharacterDB         { get; }
    private         Notifications.Notifications Notifications { get; }
    private         WalletManager               WalletManager       { get; }
    private         Node.Dogma.Dogma            Dogma               { get; }

    public slash (
        ILogger          logger, ItemFactory itemFactory, CharacterDB characterDB, Notifications.Notifications notifications, WalletManager walletManager,
        Node.Dogma.Dogma dogma
    )
    {
        Log                 = logger;
        ItemFactory         = itemFactory;
        CharacterDB         = characterDB;
        Notifications = notifications;
        WalletManager       = walletManager;
        Dogma               = dogma;

        // register commands
        this.mCommands ["create"]     = this.CreateCmd;
        this.mCommands ["createitem"] = this.CreateCmd;
        this.mCommands ["giveskills"] = this.GiveSkillCmd;
        this.mCommands ["giveskill"]  = this.GiveSkillCmd;
        this.mCommands ["giveisk"]    = this.GiveIskCmd;
    }

    private string GetCommandListForClient ()
    {
        string result = "";

        foreach (KeyValuePair <string, Action <string [], CallInformation>> pair in this.mCommands)
            result += $"'{pair.Key}',";

        return $"[{result}]";
    }

    public PyDataType SlashCmd (PyString line, CallInformation call)
    {
        if ((call.Session.Role & (int) Roles.ROLE_ADMIN) != (int) Roles.ROLE_ADMIN)
            throw new SlashError ("Only admins can run slash commands!");

        try
        {
            string [] parts = line.Value.Split (' ');

            // get the command name
            string command = parts [0].TrimStart ('/');

            // only a "/" means the client is requesting the list of commands available
            if (command.Length == 0 || this.mCommands.ContainsKey (command) == false)
                throw new SlashError ("Commands: " + this.GetCommandListForClient ());

            this.mCommands [command].Invoke (parts, call);
        }
        catch (SlashError)
        {
            throw;
        }
        catch (Exception e)
        {
            Log.Error (e.Message);
            Log.Error (e.StackTrace);

            throw new SlashError ($"Runtime error: {e.Message}");
        }

        return null;
    }

    private void GiveIskCmd (string [] argv, CallInformation call)
    {
        if (argv.Length < 3)
            throw new SlashError ("giveisk takes two arguments");

        string targetCharacter = argv [1];

        if (double.TryParse (argv [2], out double iskQuantity) == false)
            throw new SlashError ("giveisk second argument must be the ISK quantity to give");

        int targetCharacterID = 0;
        int originCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (targetCharacter == "me")
        {
            targetCharacterID = originCharacterID;
        }
        else
        {
            List <int> matches = CharacterDB.FindCharacters (targetCharacter);

            if (matches.Count > 1)
                throw new SlashError ("There's more than one character that matches the search criteria, please narrow it down");

            targetCharacterID = matches [0];
        }

        using Wallet wallet = WalletManager.AcquireWallet (targetCharacterID, Keys.MAIN);
        {
            if (iskQuantity < 0)
            {
                wallet.EnsureEnoughBalance (iskQuantity);
                wallet.CreateJournalRecord (MarketReference.GMCashTransfer, ItemFactory.OwnerSCC.ID, null, -iskQuantity);
            }
            else
            {
                wallet.CreateJournalRecord (MarketReference.GMCashTransfer, ItemFactory.OwnerSCC.ID, targetCharacterID, null, iskQuantity);
            }
        }
    }

    private void CreateCmd (string [] argv, CallInformation call)
    {
        if (argv.Length < 2)
            throw new SlashError ("create takes at least one argument");

        int typeID   = int.Parse (argv [1]);
        int quantity = 1;

        if (argv.Length > 2)
            quantity = int.Parse (argv [2]);

        if (call.Session.StationID == null)
            throw new SlashError ("Creating items can only be done at station");

        // ensure the typeID exists
        if (TypeManager.ContainsKey (typeID) == false)
            throw new SlashError ("The specified typeID doesn't exist");

        // create a new item with the correct locationID
        Station   location  = ItemFactory.GetStaticStation ((int) call.Session.StationID);
        Character character = ItemFactory.GetItem <Character> (call.Session.EnsureCharacterIsSelected ());

        Type       itemType = TypeManager [typeID];
        ItemEntity item     = ItemFactory.CreateSimpleItem (itemType, character, location, Flags.Hangar, quantity);

        item.Persist ();

        // send client a notification so they can display the item in the hangar
        Dogma.QueueMultiEvent (call.Session.EnsureCharacterIsSelected (), OnItemChange.BuildNewItemChange (item));
    }

    private static int ParseIntegerThatMightBeDecimal (string value)
    {
        int index = value.IndexOf ('.');

        if (index != -1)
            value = value.Substring (0, index);

        return int.Parse (value);
    }

    private void GiveSkillCmd (string [] argv, CallInformation call)
    {
        // TODO: NOT NODE-SAFE, MUST REIMPLEMENT TAKING THAT INTO ACCOUNT!
        if (argv.Length != 4)
            throw new SlashError ("GiveSkill must have 4 arguments");

        int characterID = call.Session.EnsureCharacterIsSelected ();

        string target    = argv [1].Trim ('"', ' ');
        string skillType = argv [2];
        int    level     = ParseIntegerThatMightBeDecimal (argv [3]);

        if (target != "me" && target != characterID.ToString ())
            throw new SlashError ("giveskill only supports me for now");

        Character character = ItemFactory.GetItem <Character> (characterID);

        if (skillType == "all")
        {
            // player wants all the skills!
            IEnumerable <KeyValuePair <int, Type>> skillTypes =
                TypeManager.Where (x => x.Value.Group.Category.ID == (int) Categories.Skill && x.Value.Published);

            Dictionary <int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

            foreach (KeyValuePair <int, Type> pair in skillTypes)
                // skill already injected, train it to the desired level
                if (injectedSkills.ContainsKey (pair.Key))
                {
                    Skill skill = injectedSkills [pair.Key];

                    skill.Level = level;
                    skill.Persist ();
                    Dogma.QueueMultiEvent (character.ID, new OnSkillTrained (skill));
                }
                else
                {
                    // skill not injected, create it, inject and done
                    Skill skill = ItemFactory.CreateSkill (
                        pair.Value, character, level,
                        SkillHistoryReason.GMGiveSkill
                    );

                    Dogma.QueueMultiEvent (character.ID, OnItemChange.BuildNewItemChange (skill));
                    Dogma.QueueMultiEvent (character.ID, new OnSkillInjected ());
                }
        }
        else
        {
            Dictionary <int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

            int skillTypeID = ParseIntegerThatMightBeDecimal (skillType);

            if (injectedSkills.ContainsKey (skillTypeID))
            {
                Skill skill = injectedSkills [skillTypeID];

                skill.Level = level;
                skill.Persist ();
                Dogma.QueueMultiEvent (character.ID, new OnSkillStartTraining (skill));
                Dogma.NotifyAttributeChange (character.ID, new [] {AttributeTypes.skillPoints, AttributeTypes.skillLevel}, skill);
                Dogma.QueueMultiEvent (character.ID, new OnSkillTrained (skill));
            }
            else
            {
                // skill not injected, create it, inject and done
                Skill skill = ItemFactory.CreateSkill (
                    TypeManager [skillTypeID], character, level,
                    SkillHistoryReason.GMGiveSkill
                );

                Dogma.QueueMultiEvent (character.ID, OnItemChange.BuildNewItemChange (skill));
                Dogma.QueueMultiEvent (character.ID, new OnSkillInjected ());
            }
        }
    }
}