using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

[ConfigSection("database")]
public class Database
{
    [ConfigValue("username")]
    public string Username { get; set; }
    [ConfigValue("password")]
    public string Password { get; set; }
    [ConfigValue("hostname")]
    public string Hostname { get; set; }
    [ConfigValue("name")]
    public string Name { get; set; }
    [ConfigValue("port", true)]
    public uint Port { get; set; } = 3306;
}