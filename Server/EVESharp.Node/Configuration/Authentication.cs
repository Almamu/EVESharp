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
    public virtual AuthenticationMessageType MessageType { get; set; } = AuthenticationMessageType.NoMessage;
    [ConfigValue("loginMessage", true)]
    public virtual string Message { get; set; }
    [ConfigValue("autoaccount", true)]
    public virtual bool Autoaccount { get; set; } = false;
    [RoleListConfigValue("role")]
    public virtual long Role { get; set; }
}