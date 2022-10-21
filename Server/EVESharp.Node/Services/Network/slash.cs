using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database.Account;
using EVESharp.Database.Characters;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions.slash;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Notifications.Skills;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using Serilog;
using Type = EVESharp.Database.Inventory.Types.Type;

namespace EVESharp.Node.Services.Network;

[MustBeCharacter]
[MustHaveRole (Roles.ROLE_ADMIN)]
public class slash : Service
{
    private readonly Dictionary <string, Action <string [], ServiceCall>> mCommands =
        new Dictionary <string, Action <string [], ServiceCall>> ();
    public override AccessLevel         AccessLevel        => AccessLevel.None;
    private         ITypes              Types              => this.Items.Types;
    private         IItems              Items              { get; }
    private         ILogger             Log                { get; }
    private         OldCharacterDB      CharacterDB        { get; }
    private         SkillDB             SkillDB            { get; }
    private         INotificationSender Notifications      { get; }
    private         IWallets            Wallets            { get; }
    private         IDogmaNotifications DogmaNotifications { get; }
    private         IDogmaItems         DogmaItems         { get; }

    public slash
    (
        ILogger             logger, IItems items, OldCharacterDB characterDB, INotificationSender notificationSender, IWallets wallets,
        IDogmaNotifications dogmaNotifications, IDogmaItems dogmaItems, SkillDB skillDB
    )
    {
        Log                     = logger;
        this.Items              = items;
        CharacterDB             = characterDB;
        Notifications           = notificationSender;
        this.Wallets            = wallets;
        this.DogmaNotifications = dogmaNotifications;
        this.DogmaItems         = dogmaItems;
        this.SkillDB            = skillDB;

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

        foreach ((string name, _) in this.mCommands)
            result += $"'{name}',";

        return $"[{result}]";
    }

    public PyDataType SlashCmd (ServiceCall call, PyString line)
    {
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

    private void GiveIskCmd (string [] argv, ServiceCall call)
    {
        if (argv.Length < 3)
            throw new SlashError ("giveisk takes two arguments");

        string targetCharacter = argv [1];

        if (double.TryParse (argv [2], out double iskQuantity) == false)
            throw new SlashError ("giveisk second argument must be the ISK quantity to give");

        int targetCharacterID = 0;
        int originCharacterID = call.Session.CharacterID;

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

        using IWallet wallet = this.Wallets.AcquireWallet (targetCharacterID, WalletKeys.MAIN);

        {
            if (iskQuantity < 0)
            {
                wallet.EnsureEnoughBalance (iskQuantity);
                wallet.CreateJournalRecord (MarketReference.GMCashTransfer, this.Items.OwnerSCC.ID, null, -iskQuantity);
            }
            else
            {
                wallet.CreateJournalRecord (MarketReference.GMCashTransfer, this.Items.OwnerSCC.ID, targetCharacterID, null, iskQuantity);
            }
        }
    }

    private void CreateCmd (string [] argv, ServiceCall call)
    {
        if (argv.Length < 2)
            throw new SlashError ("create takes at least one argument");

        int typeID   = int.Parse (argv [1]);
        int quantity = 1;

        if (argv.Length > 2)
            quantity = int.Parse (argv [2]);

        call.Session.EnsureCharacterIsInStation ();

        // ensure the typeID exists
        if (this.Types.ContainsKey (typeID) == false)
            throw new SlashError ("The specified typeID doesn't exist");

        // create a new item with the correct locationID
        DogmaItems.CreateItem <ItemEntity> (Types [typeID], call.Session.CharacterID, call.Session.StationID, Flags.Hangar, quantity);
    }

    private static int ParseIntegerThatMightBeDecimal (string value)
    {
        int index = value.IndexOf ('.');

        if (index != -1)
            value = value.Substring (0, index);

        return int.Parse (value);
    }

    private void GiveSkillCmd (string [] argv, ServiceCall call)
    {
        // TODO: NOT NODE-SAFE, MUST REIMPLEMENT TAKING THAT INTO ACCOUNT!
        if (argv.Length != 4)
            throw new SlashError ("GiveSkill must have 4 arguments");

        int characterID = call.Session.CharacterID;

        string target    = argv [1].Trim ('"', ' ');
        string skillType = argv [2];
        int    level     = ParseIntegerThatMightBeDecimal (argv [3]);

        if (target != "me" && target != characterID.ToString ())
            throw new SlashError ("giveskill only supports me for now");

        Character character = this.Items.GetItem <Character> (characterID);

        if (skillType == "all")
        {
            // player wants all the skills!
            IEnumerable <KeyValuePair <int, Type>> skillTypes =
                this.Types.Where (x => x.Value.Group.Category.ID == (int) CategoryID.Skill && x.Value.Published);

            Dictionary <int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

            foreach ((int typeID, Type type) in skillTypes)
                // skill already injected, train it to the desired level
                if (injectedSkills.ContainsKey (typeID))
                {
                    Skill skill = injectedSkills [typeID];

                    skill.Level = level;
                    skill.Persist ();
                    this.DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillTrained (skill));
                }
                else
                {
                    Skill skill = DogmaItems.CreateItem <Skill> (type, character, character, Flags.Skill, 1, true);
                    skill.Level = level;
                    skill.Persist ();
                    
                    DogmaNotifications.NotifyAttributeChange (character.ID, AttributeTypes.skillLevel, skill);
                    DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillInjected ());
                    
                    // add the skill history record too
                    SkillDB.CreateSkillHistoryRecord (
                        type, character, SkillHistoryReason.GMGiveSkill, skill.GetSkillPointsForLevel (level)
                    );
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
                
                this.DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillStartTraining (skill));
                this.DogmaNotifications.NotifyAttributeChange (character.ID, new [] {AttributeTypes.skillPoints, AttributeTypes.skillLevel}, skill);
                this.DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillTrained (skill));
            }
            else
            {
                Skill skill = DogmaItems.CreateItem <Skill> (Types [skillTypeID], character, character, Flags.Skill, 1, true);
                skill.Level = level;
                skill.Persist ();
                    
                DogmaNotifications.NotifyAttributeChange (character.ID, AttributeTypes.skillLevel, skill);
                DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillInjected ());
                    
                // add the skill history record too
                SkillDB.CreateSkillHistoryRecord (
                    Types [skillTypeID], character, SkillHistoryReason.GMGiveSkill, skill.GetSkillPointsForLevel (level)
                );
                
                this.DogmaNotifications.QueueMultiEvent (character.ID, new OnSkillInjected ());
            }
        }
    }
}