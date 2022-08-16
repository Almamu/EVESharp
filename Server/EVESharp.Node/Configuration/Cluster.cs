using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Node.Configuration;

[ConfigSection("cluster", true)]
public class Cluster
{
    [ConfigValue("url")]
    public virtual string OrchestatorURL { get; set; }
}