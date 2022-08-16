using System;
using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

public enum MachoNetMode
{
    Single = 0,
    Server = 1,
    Proxy = 2
}

[ConfigSection("machonet")]
public class MachoNet
{
    private MachoNetMode mMode = MachoNetMode.Single;
    
    [ConfigValue ("port", true)]
    public virtual ushort Port { get; set; } = 26000;
    [EnumConfigValue ("mode", typeof (MachoNetMode), true)]
    public virtual MachoNetMode Mode
    {
        get => this.mMode;
        set
        {
            if (value == MachoNetMode.Server)
                this.Port = (ushort) new Random ().Next (26001, 27000);
            
            this.mMode = value;
        }
    }
}