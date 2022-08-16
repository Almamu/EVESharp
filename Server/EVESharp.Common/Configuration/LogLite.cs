using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

[ConfigSection("loglite", true)]
public class LogLite
{
    private string mHostname;
    [ConfigValue ("hostname")]
    public virtual string Hostname
    {
        get => this.mHostname;
        set
        {
            this.mHostname = value;
            this.Enabled   = true;
        }
    }
    [ConfigValue("port")]
    public virtual string Port { get; set; }
    public virtual bool Enabled { get; set; } = false;
}