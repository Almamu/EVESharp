using System.Xml.XPath;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Chat
{
    public class LSC : Service
    {
        public LSC(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetChannels(PyDictionary namedPayload, Client client)
        {
            // TODO: SUPPORT CHAT SYSTEM
            // build an empty channel list FOR NOW
            Rowset result = new Rowset(new string []
            {
                "channelID", "ownerID", "displayName", "motd", "comparisonKey", "memberless", "password",
                "mailingList", "cspa", "temporary", "mode", "subscribed", "estimatedMemberCount"
            });

            return result;
        }

        public PyDataType GetChannels(PyInteger reload, PyDictionary namedPayload, Client client)
        {
            return this.GetChannels(namedPayload, client);
        }
    }
}