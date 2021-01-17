using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.XPath;
using Common.Constants;
using Common.Database;
using Common.Logging;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Chat
{
    public class LSC : Service
    {
        private MessagesDB MessagesDB { get; }
        private ChatDB DB { get; }
        private Channel Log;
        private ItemManager ItemManager { get; }
        private NodeContainer NodeContainer { get; }
        
        public LSC(ChatDB db, MessagesDB messagesDB, ItemManager itemManager, NodeContainer nodeContainer, Logger logger)
        {
            this.DB = db;
            this.MessagesDB = messagesDB;
            this.ItemManager = itemManager;
            this.NodeContainer = nodeContainer;
            this.Log = logger.CreateLogChannel("LSC");
        }

        public PyDataType GetChannels(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.DB.GetChannelsForCharacter((int) client.CharacterID);
        }

        public PyDataType GetChannels(PyInteger reload, PyDictionary namedPayload, Client client)
        {
            return this.GetChannels(namedPayload, client);
        }

        public PyDataType GetMembers(PyDataType channel, PyDictionary namedPayload, Client client)
        {
            int channelID = 0;

            if (channel is PyInteger integer)
                channelID = integer;
            else if (channel is PyTuple tuple)
            {
                if (tuple.Count != 1 || tuple[0] is PyTuple == false)
                {
                    Log.Error("LSC received a tuple in GetMembers that doesn't resemble anything we know");
                    return null;
                }

                PyTuple channelInfo = tuple[0] as PyTuple;

                if (channelInfo.Count != 2 || channelInfo[0] is PyString == false ||
                    channelInfo[1] is PyInteger == false)
                {
                    Log.Error("LSC received a tuple for channel in GetMembers that doesn't resemble anything we know");
                    return null;
                }

                channelID = channelInfo[1] as PyInteger;
                channelID = this.DB.GetChannelIDFromRelatedEntity(channelID);
            }
            else
            {
                throw new CustomError("The channelID is not in the correct format");
            }
                
            if (channelID == 0)
            {
                Log.Error("LSC could not determine chatID for the requested chats");
                return null;
            }
            
            return this.DB.GetChannelMembers(channelID);
        }

        protected PyDataType GenerateLSCNotification(string type, PyDataType channel, PyTuple args, Client client)
        {
            PyTuple who = new PyTuple(6)
            {
                [0] = client.AllianceID,
                [1] = client.CorporationID,
                [2] = client.CharacterID,
                [3] = client.Role,
                [4] = client.CorporationRole,
                [5] = client.WarFactionID
            };

            // this could also be a list having senderID, senderName and senderType in that order
            return new PyTuple(5)
            {
                [0] = channel,
                [1] = 1,
                [2] = type,
                [3] = who,
                [4] = args
            };
        }
        
        public PyDataType JoinChannels(PyList channels, PyInteger role, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            PyList result = new PyList();

            // TODO: SEND A NOTIFICATION TO ALL THE CHAT MEMBERS

            foreach (PyDataType channel in channels)
            {
                int channelID = 0;
                PyString typeString;
                PyDataType channelIDExtended = null;

                if (channel is PyInteger integer)
                {
                    channelID = integer;
                    
                    // get the full channel identifier
                    typeString = "global";
                    channelIDExtended = channelID;

                    if (channelID == client.CorporationID)
                    {
                        Log.Warning(
                            "Ignoring normal channel ID for corporation as it might be trying to fetch the mailing list");
                        continue;
                    }
                }
                else if (channel is PyTuple tuple)
                {
                    if (tuple.Count != 1 || tuple[0] is PyTuple == false)
                    {
                        Log.Error("LSC received a tuple in JoinChannels that doesn't resemble anything we know");
                        continue;
                    }

                    PyTuple channelInfo = tuple[0] as PyTuple;

                    if (channelInfo.Count != 2 || channelInfo[0] is PyString == false ||
                        channelInfo[1] is PyInteger == false)
                    {
                        Log.Error(
                            "LSC received a tuple for channel in JoinChannels that doesn't resemble anything we know");
                        continue;
                    }

                    channelIDExtended = tuple;
                    typeString = channelInfo[0] as PyString;
                    channelID = channelInfo[1] as PyInteger;
                }
                else
                {
                    throw new CustomError("The channelID is not in the correct format");
                }
                
                if (channelID == 0)
                {
                    Log.Error("LSC could not determine chatID for the requested chats");
                    continue;
                }

                // send notifications only on channels that should be receiving notifications
                // we don't want people in local to know about players unless they talk there
                if (typeString != "regionid" && typeString != "constellationid" && typeString != "solarsystemid2")
                {
                    PyDataType notification =
                        this.GenerateLSCNotification("JoinChannel", channelIDExtended, new PyTuple(0), client);

                    if (typeString == "global")
                    {
                        // get users in the channel that are online now
                        PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);

                        // notify them all
                        client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);  
                    }
                    else
                    {
                        // notify all players on the channel
                        client.ClusterConnection.SendNotification("OnLSC", typeString, (PyList) new PyDataType [] { channelID }, notification);                            
                    }                
                }

                try
                {
                    Row info;

                    if (typeString == "global" && channelID != (int) client.CharacterID && channelID != client.CorporationID)
                        info = this.DB.GetChannelInfo(channelID, (int) client.CharacterID);
                    else
                        info = this.DB.GetChannelInfoByRelatedEntity(channelID, (int) client.CharacterID);

                    // check if the channel must include the list of members
                    PyInteger actualChannelID = info.Line[0] as PyInteger;
                    PyString displayName = info.Line[2] as PyString;
                    string typeValue = this.DB.ChannelNameToChannelType(displayName);
                    Rowset mods = null;
                    Rowset chars = null;
                    
                    if (typeValue != "regionid" && typeValue != "constellationid" && typeValue != "solarsystemid2")
                    {
                        mods = this.DB.GetChannelMods(actualChannelID);
                        chars = this.DB.GetChannelMembers(actualChannelID);
                        
                        // the extra field is at the end
                        int extraIndex = chars.Header.Count - 1;
                
                        // ensure they all have the owner information
                        foreach (PyList row in chars.Rows)
                        {
                            // fill it with information
                            row[extraIndex] = this.DB.GetExtraInfo(row[0] as PyInteger);
                        }    
                    }
                    else
                    {
                        // build empty rowsets for channels that should not reveal anyone unless they talk
                        mods = new Rowset((PyList) new PyDataType[]
                            {"accessor", "mode", "untilWhen", "originalMode", "admin", "reason"});
                        chars = new Rowset((PyList) new PyDataType[]
                            {"charID", "corpID", "allianceID", "warFactionID", "role", "extra"});
                    }

                    result.Add(new PyTuple(3)
                    {
                        [0] = channelIDExtended,
                        [1] = 1,
                        [2] = new PyTuple(3)
                        {
                            [0] = info,
                            [1] = mods,
                            [2] = chars
                        }
                    });
                }
                catch (Exception e)
                {
                    // most of the time this indicates a destroyed channel
                    // so build a destroy notification and let the client know this channel
                    // can be removed from it's lists
                    if (typeString == "global")
                    {
                        // notify everyone in the channel only when it should
                        PyDataType notification =
                            this.GenerateLSCNotification("DestroyChannel", channelID, new PyTuple(0), client);

                        // notify all characters in the channel
                        client.ClusterConnection.SendNotification("OnLSC", "charid", (int) client.CharacterID, client, notification);
                    }
                    
                    Log.Error($"LSC could not get channel information. Error: {e.Message}");
                }
            }
            
            return result;
        }

        public PyDataType GetMyMessages(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.MessagesDB.GetMailHeaders((int) client.CharacterID);
        }
        
        public PyDataType GetRookieHelpChannel(PyDictionary namedPayload, Client client)
        {
            return ChatDB.CHANNEL_ROOKIECHANNELID;
        }

        public PyDataType SendMessage(PyInteger channelID, PyString message, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            // ensure the player is allowed to chat here
            if (this.DB.IsPlayerAllowedToChat(channelID, (int) client.CharacterID) == false)
                return null;

            PyDataType notification =
                this.GenerateLSCNotification("SendMessage", channelID, new PyTuple(1) { [0] = message }, client);

        
            // get users in the channel that are online now
            PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);

            client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);
            
            return null;
        }

        public PyDataType SendMessage(PyTuple tuple, PyString message, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            if (tuple.Count != 1 || tuple[0] is PyTuple == false)
            {
                Log.Error("LSC received a tuple in SendMessage that doesn't resemble anything we know");
                return null;
            }

            PyTuple channelInfo = tuple[0] as PyTuple;

            if (channelInfo.Count != 2 || channelInfo[0] is PyString == false ||
                channelInfo[1] is PyInteger == false)
            {
                Log.Error(
                    "LSC received a tuple for channel in SendMessage that doesn't resemble anything we know");
                return null;
            }

            PyString typeString = channelInfo[0] as PyString;
            PyInteger channelID = channelInfo[1] as PyInteger;

            if (this.DB.IsPlayerAllowedToChatOnRelatedEntity(channelID, (int) client.CharacterID) == false)
                return null;

            PyDataType notification =
                this.GenerateLSCNotification("SendMessage", tuple, new PyTuple(1) { [0] = message }, client);

            client.ClusterConnection.SendNotification("OnLSC", typeString, (PyList) new PyDataType [] { channelID }, notification);
            
            return null;
        }

        public PyDataType LeaveChannels(PyList channels, PyDataType boolUnsubscribe, PyInteger role, PyDictionary namedPayload, Client client)
        {
            foreach (PyDataType channelInfo in channels)
            {
                if (channelInfo is PyTuple == false)
                {
                    Log.Error("LSC received a channel identifier in LeaveChannels that doesn't resemble anything we know");
                    continue;
                }

                PyTuple tuple = channelInfo as PyTuple;

                if (tuple.Count != 2)
                {
                    Log.Error(
                        "LSC received a tuple for channel in LeaveChannels that doesn't resemble anything we know");
                    return null;
                }

                PyDataType channelID = tuple[0];
                PyBool announce = tuple[1] as PyBool;

                this.LeaveChannel(channelID, announce, namedPayload, client);
            }
            
            return null;
        }
        
        public PyDataType LeaveChannel(PyDataType channel, PyInteger announce, PyDictionary namedPayload, Client client)
        {
            int channelID = 0;
            PyString typeString;
            PyDataType channelIDExtended = null;

            if (channel is PyInteger integer)
            {
                channelID = integer;
                    
                // get the full channel identifier
                typeString = "global";
            }
            else if (channel is PyTuple tuple)
            {
                if (tuple.Count != 1 || tuple[0] is PyTuple == false)
                {
                    Log.Error("LSC received a tuple in JoinChannels that doesn't resemble anything we know");
                    return null;
                }

                PyTuple channelInfo = tuple[0] as PyTuple;

                if (channelInfo.Count != 2 || channelInfo[0] is PyString == false ||
                    channelInfo[1] is PyInteger == false)
                {
                    Log.Error(
                        "LSC received a tuple for channel in JoinChannels that doesn't resemble anything we know");
                    return null;
                }

                typeString = channelInfo[0] as PyString;
                channelID = channelInfo[1] as PyInteger;
            }
            else
            {
                throw new CustomError("The channelID is not in the correct format");
            }
                
            if (channelID == 0)
            {
                Log.Error("LSC could not determine chatID for the requested chats");
                return null;
            }

            // ensure we got the real channelID and not the entity ID
            if (typeString != "global")
                channelID = this.DB.GetChannelIDFromRelatedEntity(channelID);

            // make sure the character is actually in the channel
            if (this.DB.IsCharacterMemberOfChannel(channelID, (int) client.CharacterID) == false)
                return null;
            
            if (typeString != "corpid" && typeString != "solarsystemid2" && announce == 1)
            {
                // notify everyone in the channel only when it should
                PyDataType notification =
                    this.GenerateLSCNotification("LeaveChannel", channel, new PyTuple(0), client);
                
                if (typeString != "global")
                    client.ClusterConnection.SendNotification("OnLSC", typeString, (PyList) new PyDataType [] { channel }, notification);
                else
                {
                    // get users in the channel that are online now
                    PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);

                    client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);
                }
            }
            
            // remove the player from the channel
            // this.DB.LeaveChannel(channelID, (int) client.CharacterID);

            return null;
        }

        public PyDataType CreateChannel(PyString name, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (name.Length > 60)
                throw new UserError("ChatCustomChannelNameTooLong", new PyDictionary() { ["max"] = 60 });

            // create the channel in the database
            int channelID = (int) this.DB.CreateChannel((int) client.CharacterID, null, "Private Channel\\" + name, namedPayload["mailingList"] as PyBool);
            
            // join the character to this channel
            this.DB.JoinChannel(channelID, (int) client.CharacterID, ChatDB.CHATROLE_CREATOR);
            
            return null;
        }

        public PyDataType DestroyChannel(PyInteger channelID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            // ensure the character has enough permissions
            if (this.DB.IsCharacterAdminOfChannel(channelID, (int) client.CharacterID) == false)
                throw new CustomError("Only the creator of the channel can remove it");

            // get users in the channel that are online now
            PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);
            
            // remove channel off the database
            this.DB.DestroyChannel(channelID);
            
            // notify everyone in the channel only when it should
            PyDataType notification =
                this.GenerateLSCNotification("DestroyChannel", channelID, new PyTuple(0), client);

            // notify all characters in the channel
            client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);

            return null;
        }

        public PyDataType AccessControl(PyInteger channelID, PyInteger characterID, PyInteger accessLevel, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            if (this.DB.IsCharacterAdminOfChannel(channelID, (int) client.CharacterID) == false)
                return null;

            this.DB.UpdatePermissionsForCharacterOnChannel(channelID, characterID, accessLevel);

            PyTuple args = new PyTuple(6)
            {
                [0] = characterID,
                [1] = accessLevel,
                [2] = new PyNone(),
                [3] = accessLevel,
                [4] = "",
                [5] = accessLevel == ChatDB.CHATROLE_CREATOR
            };
            
            PyDataType notification = this.GenerateLSCNotification("AccessControl", channelID, args, client);
            
            // get users in the channel that are online now
            PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);

            client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);
            
            return null;
        }

        public PyDataType ForgetChannel(PyInteger channelID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            // announce leaving, important in private channels
            PyDataType notification =
                this.GenerateLSCNotification("LeaveChannel", channelID, new PyTuple(0), client);
            
            // get users in the channel that are online now
            PyList characters = this.DB.GetOnlineCharsOnChannel(channelID);

            client.ClusterConnection.SendNotification("OnLSC", "charid", characters, notification);

            this.DB.LeaveChannel(channelID, (int) client.CharacterID);
            
            return null;
        }

        public PyDataType Invite(PyInteger characterID, PyInteger channelID, PyString channelTitle, PyBool addAllowed, PyDictionary namedPayload, Client client)
        {
            PyPacket packet = new PyPacket(PyPacket.PacketType.CALL_REQ);

            // try and send a service call, will this work?
            packet.UserID = client.AccountID;
            packet.Destination = new PyAddressClient(client.AccountID, null, "LSC");
            packet.Source = new PyAddressNode(this.NodeContainer.NodeID, 1);
            packet.NamedPayload = new PyDictionary();
            packet.NamedPayload["role"] = (int) Roles.ROLE_SERVICE | (int) Roles.ROLE_REMOTESERVICE;
            packet.Payload = new PyTuple(2)
            {
                [0] = new PyTuple (2)
                {
                    [0] = 0,
                    [1] = new PySubStream(new PyTuple(4)
                    {
                        [0] = 1,
                        [1] = "ChatInvite",
                        [2] = new PyTuple(4)
                        {
                            [0] = client.CharacterID,
                            [1] = "Almamu",
                            [2] = 1,
                            [3] = channelID
                        },
                        [3] = new PyDictionary()
                    })
                },
                [1] = null
            };
            
            client.ClusterConnection.Socket.Send(packet);
            
            // invitorID, invitorName, invitorGender, channelID
            
            return null;
        }
    }
}