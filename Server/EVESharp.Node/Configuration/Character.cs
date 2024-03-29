using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Node.Configuration;

[ConfigSection("character", true)]
public class Character
{
    [ConfigValue("balance")]
    public virtual double Balance { get; set; } = 50000.0;
}