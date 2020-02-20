using ClusterControler.Configuration;
using Common.Configuration;
using IniParser;
using IniParser.Model;

namespace Configuration
{
    public class General
    {
        public Database Database { get; } = new Database();
        public LogLite LogLite { get; } = new LogLite();
        public Authentication Authentication { get; } = new Authentication();
        public FileLog FileLog { get; } = new FileLog();
        public Logging Logging { get; } = new Logging();

        public static General LoadFromFile(string filename)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filename);
            General config = new General();

            config.Database.Load(data["database"]);

            if (data.Sections.ContainsSection("autoaccount") == true)
                config.Authentication.Load(data["autoaccount"]);
            if (data.Sections.ContainsSection("loglite") == true)
                config.LogLite.Load(data["loglite"]);
            if (data.Sections.ContainsSection("logfile") == true)
                config.FileLog.Load(data["logfile"]);
            if (data.Sections.ContainsSection("logging") == true)
                config.Logging.Load(data["logging"]);

            return config;
        }
    }
}