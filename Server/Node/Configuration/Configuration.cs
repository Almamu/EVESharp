using Common.Configuration;
using IniParser;
using IniParser.Model;

namespace Node.Configuration
{
    public class General
    {
        public Common.Configuration.Database Database { get; private set; } = new Common.Configuration.Database();
        public Proxy Proxy { get; private set; } = new Proxy();
        public Authentication Authentication { get; private set; } = new Authentication();
        public LogLite LogLite { get; private set; } = new LogLite();
        public FileLog FileLog { get; private set; } = new FileLog();
        public Logging Logging { get; } = new Logging();
        public Character Character { get; } = new Character();

        public static General LoadFromFile(string filename)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filename);
            General config = new General();

            config.Database.Load(data["database"]);
            config.Proxy.Load(data["proxy"]);

            if (data.Sections.ContainsSection("authentication") == true)
                config.Authentication.Load(data["authentication"]);
            if (data.Sections.ContainsSection("loglite") == true)
                config.LogLite.Load(data["loglite"]);
            if (data.Sections.ContainsSection("logfile") == true)
                config.FileLog.Load(data["logfile"]);
            if (data.Sections.ContainsSection("logging") == true)
                config.Logging.Load(data["logging"]);
            if (data.Sections.ContainsSection("character") == true)
                config.Character.Load(data["character"]);

            return config;
        }
    }
}