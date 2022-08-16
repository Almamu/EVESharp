using EVESharp.Common.Configuration;
using EVESharp.EVE.Configuration;

namespace EVESharp.Node.Configuration;

public class General
{
    public virtual Authentication                Authentication { get; set; }
    public virtual Common.Configuration.Database Database       { get; set; }
    public virtual MachoNet                      MachoNet       { get; set; }
    public virtual LogLite                       LogLite        { get; set; }
    public virtual FileLog                       FileLog        { get; set; }
    public virtual Logging                       Logging        { get; set; }
    public virtual Character                     Character      { get; set; }
    public virtual Cluster                       Cluster        { get; set; }
}