using IniParser.Model;

namespace EVESharp.Common.Configuration;

public class FileLog
{
    public string Directory { get; private set; }
    public string LogFile   { get; private set; }

    public bool Enabled { get; private set; }

    public void Load (KeyDataCollection section)
    {
        Enabled   = true;
        Directory = section ["directory"];
        LogFile   = section ["logfile"];
    }
}