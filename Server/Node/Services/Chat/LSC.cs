using System;
using System.Runtime.InteropServices;
using System.Xml.XPath;
using Common.Database;
using Common.Logging;
using Node.Database;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Chat
{
    public class LSC : Service
    {
        private MessagesDB mMessagesDB = null;
        private ChatDB mDB = null;
        private Channel Log;
        
        public LSC(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new ChatDB(db);
            this.mMessagesDB = new MessagesDB(db);
            this.Log = this.ServiceManager.Container.Logger.CreateLogChannel("LSC");
        }

        public PyDataType GetChannels(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.mDB.GetChannelsForCharacter((int) client.CharacterID);
        }

        public PyDataType GetChannels(PyInteger reload, PyDictionary namedPayload, Client client)
        {
            return this.GetChannels(namedPayload, client);
        }
        
        public PyDataType JoinChannels(PyList channels, PyInteger role, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            PyList notifications = new PyList();

            // TODO: SEND A NOTIFICATION TO ALL THE CHAT MEMBERS

            foreach (PyDataType channel in channels)
            {
                int channelID = 0;
                PyDataType channelIDExtended = null;

                if (channel is PyInteger integer)
                    channelIDExtended = channelID = integer;
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
                    channelID = channelInfo[1] as PyInteger;
                }

                if (channelID == 0)
                {
                    Log.Error("LSC could not determine chatID for the requested chats");
                    continue;
                }

                try
                {
                    Rowset mods = this.mDB.GetChannelMods(channelID);
                    Rowset chars = this.mDB.GetChannelMembers(channelID);
                    PyDataType info = this.mDB.GetChannelInfo(channelID, (int) client.CharacterID);
                
                    // the extra field is at the end
                    int extraIndex = chars.Header.Count - 1;
                
                    // ensure they all have the owner information
                    foreach (PyList row in chars.Rows)
                    {
                        // fill it with information
                        row[extraIndex] = this.mDB.GetExtraInfo(row[0] as PyInteger);
                    }

                    notifications.Add(new PyTuple(new PyDataType[]
                            {
                                channelIDExtended, 1, new PyTuple(new PyDataType [] { info, mods, chars })
                            }
                        )
                    );
                }
                catch (Exception e)
                {
                    Log.Error($"LSC could not get channel information. Error: {e.Message}");
                }
            }
            
            return notifications;
        }

        public PyDataType GetMyMessages(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.mMessagesDB.GetMailHeaders((int) client.CharacterID);
        }
        
        public PyDataType GetRookieHelpChannel(PyDictionary namedPayload, Client client)
        {
            return ChatDB.CHANNEL_ROOKIECHANNELID;
        }
    }
}