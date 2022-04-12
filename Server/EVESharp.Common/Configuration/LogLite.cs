using IniParser.Model;

namespace EVESharp.Common.Configuration;

public class LogLite
{
    public string Hostname { get; private set; }
    public string Port     { get; private set; }
    public bool   Enabled  { get; private set; }

    public void Load (KeyDataCollection section)
    {
        Enabled  = true;
        Hostname = section ["hostname"];
        Port     = section ["port"];
    }
}