using System;
using IniParser.Model;

namespace Node.Configuration
{
    public enum AuthenticationMessageType
    {
        NoMessage = 0,
        HTMLMessage = 1
    }
    
    public class Authentication
    {
        public AuthenticationMessageType MessageType { get; private set; }
        public string Message { get; private set; }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("loginMessageType") == false)
            {
                this.MessageType = AuthenticationMessageType.NoMessage;
                return;
            }

            string value = section["loginMessageType"].ToUpper();

            switch (value)
            {
                case "MESSAGE":
                    if (section.ContainsKey("loginMessage") == false)
                        throw new Exception(
                            "Authentication service configuration must specify an HTML message"
                        );
                    this.Message = section["loginMessage"];
                    this.MessageType = AuthenticationMessageType.HTMLMessage;
                    break;
                case "NONE":
                default:
                    this.MessageType = AuthenticationMessageType.NoMessage;
                    break;
            }
        }
    }
}