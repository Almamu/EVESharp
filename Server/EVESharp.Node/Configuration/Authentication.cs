using EVESharp.Common.Configuration.Attributes;
using EVESharp.EVE.Configuration.Attributes;

namespace EVESharp.Node.Configuration;

public enum AuthenticationMessageType
{
    NoMessage = 0,
    Message = 1
}
[ConfigSection("authentication", true)]
public class Authentication
{
    [EnumConfigValue ("loginMessageType", typeof (AuthenticationMessageType), true)]
    public AuthenticationMessageType MessageType { get; set; } = AuthenticationMessageType.NoMessage;
    [ConfigValue("loginMessage", true)]
    public string Message { get; set; }
    [ConfigValue("autoaccount", true)]
    public bool Autoaccount { get; set; } = false;
    [RoleListConfigValue("role")]
    public long Role { get; set; }
}