using System.Collections.Generic;
using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

[ConfigSection("logging", true)]
public class Logging
{
    [ConfigValue("force")]
    public List<string> EnableChannels { get; set; }
}