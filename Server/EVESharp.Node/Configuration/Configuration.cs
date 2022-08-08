using EVESharp.Common.Configuration;
using EVESharp.EVE.Configuration;
using IniParser;
using IniParser.Model;

namespace EVESharp.Node.Configuration;

public class General
{
    public Common.Configuration.Database Database       { get; } = new Common.Configuration.Database ();
    public MachoNet                      MachoNet       { get; } = new MachoNet ();
    public Authentication                Authentication { get; } = new Authentication ();
    public LogLite                       LogLite        { get; } = new LogLite ();
    public FileLog                       FileLog        { get; } = new FileLog ();
    public Logging                       Logging        { get; } = new Logging ();
    public Character                     Character      { get; } = new Character ();
    public Cluster                       Cluster        { get; } = new Cluster ();

    public static General LoadFromFile (string filename)
    {
        FileIniDataParser parser = new FileIniDataParser ();
        IniData           data   = parser.ReadFile (filename);
        General           config = new General ();

        config.Database.Load (data ["database"]);
        config.MachoNet.Load (data ["machonet"]);
        config.Cluster.Load (data ["cluster"]);

        if (data.Sections.ContainsSection ("authentication"))
            config.Authentication.Load (data ["authentication"]);
        if (data.Sections.ContainsSection ("loglite"))
            config.LogLite.Load (data ["loglite"]);
        if (data.Sections.ContainsSection ("logfile"))
            config.FileLog.Load (data ["logfile"]);
        if (data.Sections.ContainsSection ("logging"))
            config.Logging.Load (data ["logging"]);
        if (data.Sections.ContainsSection ("character"))
            config.Character.Load (data ["character"]);

        return config;
    }
}