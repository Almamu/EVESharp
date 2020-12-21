using System.Collections.Generic;
using Common.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class MessagesDB : DatabaseAccessor
    {
        public MessagesDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType GetMailHeaders(int channelID)
        {
            return Database.PrepareRowsetQuery(
	            "SELECT channelID, messageID, senderID, subject, created, `read` FROM eveMail WHERE channelID = @channelID",
	            new Dictionary<string, object>()
	            {
		            {"@channelID", channelID}
	            }
	        );
        }
    }
}