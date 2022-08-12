using EVESharp.Common.Configuration;
using EVESharp.EVE.Configuration;

namespace EVESharp.Node.Configuration;

public class General
{
    public Authentication                Authentication { get; set; }
    public Common.Configuration.Database Database       { get; set; }
    public MachoNet                      MachoNet       { get; set; }
    public LogLite                       LogLite        { get; set; }
    public FileLog                       FileLog        { get; set; }
    public Logging                       Logging        { get; set; }
    public Character                     Character      { get; set; }
    public Cluster                       Cluster        { get; set; }
}