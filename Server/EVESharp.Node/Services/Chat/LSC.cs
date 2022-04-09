using System;
using System.IO;
using EVESharp.EVE.Client.Exceptions.LSC;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Node.Chat;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Chat;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Services.Chat;

public class LSC : Service
{
    /// <summary>
    /// The type of notification used through the whole LSC service
    /// </summary>
    private const string NOTIFICATION_TYPE = "OnLSC";
    private         ILogger     Log         { get; }
    public override AccessLevel AccessLevel => AccessLevel.Location;

    private MessagesDB          MessagesDB          { get; }
    private ChatDB              DB                  { get; }
    private CharacterDB         CharacterDB         { get; }
    private ItemFactory         ItemFactory         { get; }
    private NodeContainer       NodeContainer       { get; }
    private NotificationManager NotificationManager { get; }
    private MailManager         MailManager         { get; }

    public LSC (
        ChatDB              db, MessagesDB messagesDB, CharacterDB characterDB, ItemFactory itemFactory, NodeContainer nodeContainer, ILogger logger,
        NotificationManager notificationManager, MailManager mailManager
    )
    {
        DB                  = db;
        MessagesDB          = messagesDB;
        CharacterDB         = characterDB;
        ItemFactory         = itemFactory;
        NodeContainer       = nodeContainer;
        NotificationManager = notificationManager;
        MailManager         = mailManager;
        Log                 = logger;
    }

    private void ParseTupleChannelIdentifier (PyTuple tuple, out int channelID, out string channelType, out int? entityID)
    {
        if (tuple.Count != 1 || tuple [0] is PyTuple == false)
            throw new InvalidDataException ("LSC received a wrongly formatted channel identifier");

        PyTuple channelInfo = tuple [0] as PyTuple;

        if (channelInfo.Count != 2 || channelInfo [0] is PyString == false || channelInfo [1] is PyInteger == false)
            throw new InvalidDataException ("LSC received a wrongly formatted channel identifier");

        channelType = channelInfo [0] as PyString;
        entityID    = channelInfo [1] as PyInteger;

        channelID = DB.GetChannelIDFromRelatedEntity ((int) entityID);

        if (channelID < 0)
            throw new InvalidDataException ("LSC received a wrongly formatted channel identifier (negative entityID)");
    }

    private void ParseChannelIdentifier (PyDataType channel, out int channelID, out string channelType, out int? entityID)
    {
        switch (channel)
        {
            case PyInteger integer:
                channelID = integer;
                // positive channel ids are entity ids, negatives are custom user channels
                entityID = null;
                if (channelID > ChatDB.MIN_CHANNEL_ENTITY_ID && channelID < ChatDB.MAX_CHANNEL_ENTITY_ID)
                    entityID = channelID;
                // get the full channel identifier
                channelType = ChatDB.CHANNEL_TYPE_NORMAL;

                break;
            case PyTuple tuple:
                this.ParseTupleChannelIdentifier (tuple, out channelID, out channelType, out entityID);

                break;
            default:
                throw new InvalidDataException ("LSC received a wrongly formatted channel identifier");
        }

        // ensure the channelID is the correct one and not an entityID
        if (entityID is not null)
            channelID = DB.GetChannelIDFromRelatedEntity ((int) entityID, channelID == entityID);

        if (channelID == 0)
            throw new InvalidDataException ("LSC could not determine chatID for the requested chats");
    }

    private void ParseChannelIdentifier (PyDataType channel, out int channelID, out string channelType)
    {
        this.ParseChannelIdentifier (channel, out channelID, out channelType, out _);
    }

    public PyDataType GetChannels (CallInformation call)
    {
        return DB.GetChannelsForCharacter (call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID);
    }

    public PyDataType GetChannels (PyInteger reload, CallInformation call)
    {
        return this.GetChannels (call);
    }

    public PyDataType GetMembers (PyDataType channel, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        int    channelID;
        string channelType;

        try
        {
            this.ParseChannelIdentifier (channel, out channelID, out channelType);
        }
        catch (InvalidDataException)
        {
            Log.Error ("Error parsing channel identifier for GetMembers");

            return null;
        }

        return DB.GetChannelMembers (channelID, callerCharacterID);
    }

    private PyTuple GetChannelInformation (
        string channelType, int channelID, int? entityID, int callerCharacterID, PyDataType channelIDExtended, CallInformation call
    )
    {
        Row info;

        if (channelID < ChatDB.MIN_CHANNEL_ENTITY_ID || entityID is null || channelID >= ChatDB.MAX_CHANNEL_ENTITY_ID)
            info = DB.GetChannelInfo (channelID, callerCharacterID);
        else
            info = DB.GetChannelInfoByRelatedEntity ((int) entityID, callerCharacterID, channelID == entityID);

        // check if the channel must include the list of members
        PyInteger actualChannelID = info.Line [0] as PyInteger;
        PyString  displayName     = info.Line [2] as PyString;
        string    typeValue       = DB.ChannelNameToChannelType (displayName);
        Rowset    mods            = null;
        Rowset    chars           = null;

        if (typeValue != ChatDB.CHANNEL_TYPE_REGIONID && typeValue != ChatDB.CHANNEL_TYPE_CONSTELLATIONID && typeValue != ChatDB.CHANNEL_TYPE_SOLARSYSTEMID2)
        {
            mods  = DB.GetChannelMods (actualChannelID);
            chars = DB.GetChannelMembers (actualChannelID, callerCharacterID);

            // the extra field is at the end
            int extraIndex = chars.Header.Count - 1;

            // ensure they all have the owner information
            foreach (PyList row in chars.Rows)
                // fill it with information
                row [extraIndex] = DB.GetExtraInfo (row [0] as PyInteger);
        }
        else
        {
            // build empty rowsets for channels that should not reveal anyone unless they talk
            mods = new Rowset (
                new PyList <PyString> (6)
                {
                    [0] = "accessor",
                    [1] = "mode",
                    [2] = "untilWhen",
                    [3] = "originalMode",
                    [4] = "admin",
                    [5] = "reason"
                }
            );
            chars = new Rowset (
                new PyList <PyString> (6)
                {
                    [0] = "charID",
                    [1] = "corpID",
                    [2] = "allianceID",
                    [3] = "warFactionID",
                    [4] = "role",
                    [5] = "extra"
                }
            );
        }

        return new PyTuple (3)
        {
            [0] = channelIDExtended,
            [1] = 1,
            [2] = new PyTuple (3)
            {
                [0] = info,
                [1] = mods,
                [2] = chars
            }
        };
    }

    public PyList <PyTuple> JoinChannels (PyList channels, PyInteger role, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        PyList <PyTuple> result = new PyList <PyTuple> ();

        foreach (PyDataType channel in channels)
        {
            int        channelID;
            string     channelType;
            int?       entityID;
            PyDataType channelIDExtended = null;

            try
            {
                this.ParseChannelIdentifier (channel, out channelID, out channelType, out entityID);
            }
            catch (InvalidDataException ex)
            {
                throw new LSCCannotJoin ($"The specified channel cannot be found ({ex.Message}): " + PrettyPrinter.FromDataType (channel));
            }

            if (channelType == ChatDB.CHANNEL_TYPE_NORMAL)
                channelIDExtended = channelID;
            else
                channelIDExtended = new PyTuple (1)
                {
                    [0] = new PyTuple (2)
                    {
                        [0] = channelType,
                        [1] = entityID
                    }
                };

            // send notifications only on channels that should be receiving notifications
            // we don't want people in local to know about players unless they talk there
            if (channelType != ChatDB.CHANNEL_TYPE_REGIONID && channelType != ChatDB.CHANNEL_TYPE_CONSTELLATIONID &&
                channelType != ChatDB.CHANNEL_TYPE_SOLARSYSTEMID2)
            {
                OnLSC joinNotification = new OnLSC (call.Session, "JoinChannel", channelIDExtended, new PyTuple (0));

                if (channelType == ChatDB.CHANNEL_TYPE_NORMAL)
                {
                    if (channelID < ChatDB.MIN_CHANNEL_ENTITY_ID)
                    {
                        // get users in the channel that are online now
                        PyList <PyInteger> characters = DB.GetOnlineCharsOnChannel (channelID);

                        // notify them all
                        NotificationManager.NotifyCharacters (characters, joinNotification);
                    }
                }
                else
                {
                    // notify all players on the channel
                    NotificationManager.SendNotification (channelType, new PyList (1) {[0] = entityID}, joinNotification);
                }
            }

            try
            {
                result.Add (this.GetChannelInformation (channelType, channelID, entityID, callerCharacterID, channelIDExtended, call));
            }
            catch (Exception e)
            {
                // most of the time this indicates a destroyed channel
                // so build a destroy notification and let the client know this channel
                // can be removed from it's lists
                if (channelType == ChatDB.CHANNEL_TYPE_NORMAL && channelID != entityID)
                    // notify all characters in the channel
                    NotificationManager.NotifyCharacter (callerCharacterID, new OnLSC (call.Session, "DestroyChannel", channelID, new PyTuple (0)));

                Log.Error ($"LSC could not get channel information. Error: {e.Message}");
            }
        }

        return result;
    }

    public PyDataType GetMyMessages (CallInformation call)
    {
        return MessagesDB.GetMailHeaders (call.Session.EnsureCharacterIsSelected ());
    }

    public PyDataType GetRookieHelpChannel (CallInformation call)
    {
        return ChatDB.CHANNEL_ROOKIECHANNELID;
    }

    public PyDataType SendMessage (PyDataType channel, PyString message, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        int    channelID;
        int?   entityID;
        string channelType;

        try
        {
            this.ParseChannelIdentifier (channel, out channelID, out channelType, out entityID);
        }
        catch (InvalidDataException)
        {
            throw new LSCCannotSendMessage ("Cannot get channel information");
        }

        // ensure the player is allowed to chat in there
        if (channelType == ChatDB.CHANNEL_TYPE_NORMAL && DB.IsPlayerAllowedToChat (channelID, callerCharacterID) == false)
            throw new LSCCannotSendMessage ("Insufficient permissions");
        if (channelType != ChatDB.CHANNEL_TYPE_NORMAL && DB.IsPlayerAllowedToChatOnRelatedEntity ((int) entityID, callerCharacterID) == false)
            throw new LSCCannotSendMessage ("Insufficient permissions");

        PyTuple notificationBody = new PyTuple (1) {[0] = message};

        if (channelType == ChatDB.CHANNEL_TYPE_NORMAL)
        {
            NotificationManager.NotifyCharacters (
                DB.GetOnlineCharsOnChannel (channelID),
                new OnLSC (call.Session, "SendMessage", channelID, notificationBody)
            );
        }
        else
        {
            PyTuple identifier = new PyTuple (1)
            {
                [0] = new PyTuple (2)
                {
                    [0] = channelType,
                    [1] = entityID
                }
            };

            NotificationManager.SendNotification (
                channelType,
                new PyList (1) {[0] = entityID},
                new OnLSC (call.Session, "SendMessage", identifier, notificationBody)
            );
        }

        return null;
    }

    public PyDataType LeaveChannels (PyList channels, PyDataType boolUnsubscribe, PyInteger role, CallInformation call)
    {
        foreach (PyDataType channelInfo in channels)
        {
            if (channelInfo is PyTuple == false)
            {
                Log.Error ("LSC received a channel identifier in LeaveChannels that doesn't resemble anything we know");

                continue;
            }

            PyTuple tuple = channelInfo as PyTuple;

            if (tuple.Count != 2)
            {
                Log.Error ("LSC received a tuple for channel in LeaveChannels that doesn't resemble anything we know");

                return null;
            }

            PyDataType channelID = tuple [0];
            PyBool     announce  = tuple [1] as PyBool;

            this.LeaveChannel (channelID, announce, call);
        }

        return null;
    }

    public PyDataType LeaveChannel (PyDataType channel, PyInteger announce, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        int    channelID;
        string channelType;

        try
        {
            this.ParseChannelIdentifier (channel, out channelID, out channelType);
        }
        catch (InvalidDataException)
        {
            Log.Error ("Error parsing channel identifier for LeaveChannel");

            return null;
        }

        // make sure the character is actually in the channel
        if (DB.IsCharacterMemberOfChannel (channelID, callerCharacterID) == false)
            return null;

        // TODO: ENSURE THIS CHECK IS CORRECT, FOR SOME REASON IT DOES NOT SIT RIGHT WITH ME (Almamu)
        if (channelType != ChatDB.CHANNEL_TYPE_CORPID && channelType != ChatDB.CHANNEL_TYPE_SOLARSYSTEMID2 && announce == 1)
        {
            // notify everyone in the channel only when it should
            OnLSC leaveNotification = new OnLSC (call.Session, "LeaveChannel", channel, new PyTuple (0));

            if (channelType != ChatDB.CHANNEL_TYPE_NORMAL)
                NotificationManager.SendNotification (channelType, new PyList (1) {[0] = channel}, leaveNotification);
            else
                NotificationManager.NotifyCharacters (
                    DB.GetOnlineCharsOnChannel (channelID),
                    leaveNotification
                );
        }

        return null;
    }

    public PyDataType CreateChannel (PyString name, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (name.Length > 60)
            throw new ChatCustomChannelNameTooLong (60);

        bool mailingList = false;

        if (call.NamedPayload.ContainsKey ("mailingList"))
            mailingList = call.NamedPayload ["mailingList"] as PyBool;

        // create the channel in the database
        int channelID = (int) DB.CreateChannel (callerCharacterID, null, "Private Channel\\" + name, mailingList);

        // join the character to this channel
        DB.JoinChannel (channelID, callerCharacterID, ChatDB.CHATROLE_CREATOR);

        Rowset mods  = DB.GetChannelMods (channelID);
        Rowset chars = DB.GetChannelMembers (channelID, callerCharacterID);

        // the extra field is at the end
        int extraIndex = chars.Header.Count - 1;

        // ensure they all have the owner information
        foreach (PyList row in chars.Rows)
            // fill it with information
            row [extraIndex] = DB.GetExtraInfo (row [0] as PyInteger);

        // retrieve back the information about the characters as there is ONE character in here
        // return the normal channel information
        return new PyTuple (3)
        {
            [0] = DB.GetChannelInfo (channelID, callerCharacterID),
            [1] = mods,
            [2] = chars
        };
    }

    public PyDataType DestroyChannel (PyInteger channelID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        // ensure the character has enough permissions
        if (DB.IsCharacterAdminOfChannel (channelID, callerCharacterID) == false)
            throw new LSCCannotDestroy ("Insufficient permissions");

        // get users in the channel that are online now
        PyList <PyInteger> characters = DB.GetOnlineCharsOnChannel (channelID);

        // remove channel off the database
        DB.DestroyChannel (channelID);

        // notify all characters in the channel
        NotificationManager.NotifyCharacters (characters, new OnLSC (call.Session, "DestroyChannel", channelID, new PyTuple (0)));

        return null;
    }

    public PyDataType AccessControl (PyInteger channelID, PyInteger characterID, PyInteger accessLevel, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (DB.IsCharacterOperatorOrAdminOfChannel (channelID, callerCharacterID) == false)
            throw new LSCCannotAccessControl ("Insufficient permissions");

        DB.UpdatePermissionsForCharacterOnChannel (channelID, characterID, accessLevel);

        PyTuple args = new PyTuple (6)
        {
            [0] = characterID,
            [1] = accessLevel,
            [2] = null,
            [3] = accessLevel,
            [4] = "",
            [5] = accessLevel == ChatDB.CHATROLE_CREATOR
        };

        // get users in the channel that are online now
        NotificationManager.NotifyCharacters (
            DB.GetOnlineCharsOnChannel (channelID),
            new OnLSC (call.Session, "AccessControl", channelID, args)
        );

        // TODO: CHECK IF THIS IS A CHARACTER'S ADDRESS BOOK AND CHECK FOR OTHER CHARACTER'S ADDRESSBOOK STATUS
        // TODO: TO ENSURE THEY DON'T HAVE US BLOCKED

        return null;
    }

    public PyDataType ForgetChannel (PyInteger channelID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        // announce leaving, important in private channels
        NotificationManager.NotifyCharacters (
            DB.GetOnlineCharsOnChannel (channelID),
            new OnLSC (call.Session, "LeaveChannel", channelID, new PyTuple (0))
        );

        DB.LeaveChannel (channelID, callerCharacterID);

        return null;
    }

    public void InviteAnswerCallback (RemoteCall callInfo, PyDataType result)
    {
        InviteExtraInfo call = callInfo.ExtraInfo as InviteExtraInfo;

        if (result is PyString answer)
        {
            // this user's character might not be in the service
            // so fetch the name from the database
            // TODO: SUPPORT REMOTE SERVICE CALLS AGAIN
            /*
            call.OriginalCall.Client.SendException(
                call.OriginalCall,
                new UserError(
                    answer,
                    new PyDictionary
                    {
                        ["channel"] = this.DB.GetChannelName(call.ChannelID),
                        ["char"] = this.CharacterDB.GetCharacterName(call.ToCharacterID)
                    }
                )
            );
            */
        }

        // return an empty response to the original calling client, this should get mechanism going for the JoinChannel notification
        // TODO: SUPPORT REMOTE SERVICE CALLS AGAIN
        // callInfo.Client.Transport.SendCallResult(call.OriginalCall, null);

        // character has accepted, notify all users of the channel
        string channelType = DB.GetChannelType (call.ChannelID);

        // you should only be able to invite to global channels as of now
        // TODO: CORP CHANNELS SHOULD BE SUPPORTED TOO
        if (channelType == ChatDB.CHANNEL_TYPE_NORMAL)
            // notify all the characters in the channel
            NotificationManager.NotifyCharacters (
                DB.GetOnlineCharsOnChannel (call.ChannelID),
                new OnLSC (callInfo.Session, "JoinChannel", call.ChannelID, new PyTuple (0))
            );
    }

    public void InviteTimeoutCallback (RemoteCall callInfo)
    {
        // if the call timed out the character is not connected
        InviteExtraInfo call = callInfo.ExtraInfo as InviteExtraInfo;

        // TODO: SUPPORT REMOTE SERVICE CALLS AGAIN
        /*
        call.OriginalCall.Client.SendException(
            call.OriginalCall,
            new ChtCharNotReachable(call.ToCharacterID)
        );
        */
    }

    public PyDataType Invite (PyInteger characterID, PyInteger channelID, PyString channelTitle, PyBool addAllowed, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        try
        {
            if (characterID == callerCharacterID)
                throw new ChtCannotInviteSelf ();

            if (ItemFactory.IsNPC (characterID))
                throw new ChtNPC (characterID);

            // ensure our character has admin perms first
            if (DB.IsCharacterOperatorOrAdminOfChannel (channelID, callerCharacterID) == false)
                throw new ChtWrongRole (DB.GetChannelName (channelID), "Operator");

            // ensure the character is not there already
            if (DB.IsCharacterMemberOfChannel (channelID, characterID))
                throw new ChtAlreadyInChannel (characterID);

            // TODO: THIS WONT WORK ON MULTIPLE-NODES ENVIRONMENTS, NEEDS FIXING
            Character character = ItemFactory.GetItem <Character> (callerCharacterID);

            PyTuple args = new PyTuple (4)
            {
                [0] = callerCharacterID,
                [1] = character.Name,
                [2] = character.Gender,
                [3] = channelID
            };

            InviteExtraInfo info = new InviteExtraInfo
            {
                OriginalCall    = call,
                Arguments       = args,
                ChannelID       = channelID,
                ToCharacterID   = characterID,
                FromCharacterID = callerCharacterID
            };

            // no timeout for this call
            // TODO: SUPPORT REMOTE SERVICE CALLS AGAIN
            /*
            call.Client.Transport.SendServiceCall(
                "LSC", "ChatInvite", args, new PyDictionary(),
                InviteAnswerCallback, InviteTimeoutCallback,
                info, ProvisionalResponse.DEFAULT_TIMEOUT - 5
            );
            */

            // subscribe the user to the chat
            DB.JoinChannel (channelID, characterID);
        }
        catch (ArgumentOutOfRangeException)
        {
            Log.Warning ("Trying to invite a non-online character, aborting...");
        }

        // return SOMETHING to the client with the provisional data
        // the real answer will come later on
        throw new ProvisionalResponse (new PyString ("OnDummy"), new PyTuple (0));
    }

    public PyDataType Page (PyList destinationMailboxes, PyString subject, PyString message, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        // TODO: AS IT IS RIGHT NOW THE USER CAN INJECT HTML IF THE CALL IS DONE MANUALL (THROUGH CUSTOM CODE OR IMPLEMENTING THE FULL GAME PROTOCOL)
        // TODO: THE HTML IT SUPPORTS IS NOT THAT BIG, BUT BETTER BE SAFE AND DO SOME DETECTIONS HERE TO PREVENT HTML FROM BEING USED!

        MailManager.SendMail (callerCharacterID, destinationMailboxes.GetEnumerable <PyInteger> (), subject, message);

        return null;
    }

    public PyDataType GetMessageDetails (PyInteger channelID, PyInteger messageID, CallInformation call)
    {
        // ensure the player is allowed to read messages off this mail list
        if (DB.IsPlayerAllowedToRead (channelID, call.Session.EnsureCharacterIsSelected ()) == false)
            return null;

        return MessagesDB.GetMessageDetails (channelID, messageID);
    }

    public PyDataType MarkMessagesRead (PyList messageIDs, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        MessagesDB.MarkMessagesRead (callerCharacterID, messageIDs.GetEnumerable <PyInteger> ());

        return null;
    }

    public PyDataType DeleteMessages (PyInteger mailboxID, PyList messageIDs, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        MessagesDB.DeleteMessages (callerCharacterID, mailboxID, messageIDs.GetEnumerable <PyInteger> ());

        return null;
    }

    private class InviteExtraInfo
    {
        public CallInformation OriginalCall    { get; set; }
        public int             FromCharacterID { get; set; }
        public int             ToCharacterID   { get; set; }
        public int             ChannelID       { get; set; }
        public PyTuple         Arguments       { get; set; }
    }
}