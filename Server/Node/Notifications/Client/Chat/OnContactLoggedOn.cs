using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Chat
{
    public class OnContactLoggedOn : PyNotification
    {
        private const string NOTIFICATION_NAME = "OnContactLoggedOn";
        
        public int CharacterID { get; init; }
        
        public OnContactLoggedOn(int characterID) : base(NOTIFICATION_NAME)
        {
            this.CharacterID = characterID;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.CharacterID
            };
        }
    }
}