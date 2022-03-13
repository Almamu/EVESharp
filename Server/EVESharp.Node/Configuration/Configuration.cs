using EVESharp.Common.Configuration;
using IniParser;
using IniParser.Model;
using SimpleInjector;

namespace EVESharp.Node.Configuration
{
    public class General
    {
        public EVESharp.Common.Configuration.Database Database { get; private set; } = new EVESharp.Common.Configuration.Database();
        public MachoNet MachoNet { get; private set; } = new MachoNet();
        public Authentication Authentication { get; private set; } = new Authentication();
        public LogLite LogLite { get; private set; } = new LogLite();
        public FileLog FileLog { get; private set; } = new FileLog();
        public Logging Logging { get; } = new Logging();
        public Character Character { get; } = new Character();
        public Cluster Cluster { get; } = new Cluster();

        public static General LoadFromFile(string filename, Container container)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filename);
            General config = new General();

            config.Database.Load(data["database"]);
            config.MachoNet.Load(data["machonet"]);
            config.Cluster.Load(data["cluster"]);

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

            // register all the configuration options as dependencies available
            container.RegisterInstance(config.Database);
            container.RegisterInstance(config.MachoNet);
            container.RegisterInstance(config.Authentication);
            container.RegisterInstance(config.LogLite);
            container.RegisterInstance(config.FileLog);
            container.RegisterInstance(config.Logging);
            container.RegisterInstance(config.Character);
            
            return config;
        }
    }
}