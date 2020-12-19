using System.Xml.XPath;
using Common.Database;
using Node.Database;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Chat
{
    public class LSC : Service
    {
        
        private ChatDB mDB = null;
        
        public LSC(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new ChatDB(db);
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

        public PyDataType GetRookieHelpChannel(PyDictionary namedPayload, Client client)
        {
            return ChatDB.CHANNEL_ROOKIECHANNELID;
        }
    }
}