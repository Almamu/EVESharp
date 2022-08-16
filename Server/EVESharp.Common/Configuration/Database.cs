using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

[ConfigSection("database")]
public class Database
{
    [ConfigValue("username")]
    public virtual string Username { get; set; }
    [ConfigValue("password")]
    public virtual string Password { get; set; }
    [ConfigValue("hostname")]
    public virtual string Hostname { get; set; }
    [ConfigValue("name")]
    public virtual string Name { get; set; }
    [ConfigValue("port", true)]
    public virtual uint Port { get; set; } = 3306;
}