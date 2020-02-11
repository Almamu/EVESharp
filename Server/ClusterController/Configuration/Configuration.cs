using System;
using System.IO;
using ClusterControler.Configuration;
using Common;
using Common.Configuration;
using IniParser;
using IniParser.Model;

namespace Configuration
{
    public class General
    {
        public Database Database { get; private set; } = new Database();
        public LogLite LogLite { get; private set; } = new LogLite();
        public Authentication Authentication { get; private set; } = new Authentication();
        public FileLog FileLog { get; private set; } = new FileLog();

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

            return config;
        }
    }
}