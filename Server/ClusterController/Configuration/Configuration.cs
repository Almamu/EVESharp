using System;
using System.IO;
using Common;
using Common.Configuration;
using IniParser;
using IniParser.Model;

namespace Configuration
{
    public class General
    {
        private Database m_Database = new Database ();

        private Logging m_Logging = new Logging ();
        
        public Database Database
        {
            get { return this.m_Database; }
            set { this.m_Database = value; }
        }

        public Logging Logging
        {
            get { return this.m_Logging; }
            set { this.m_Logging = value; }
        }

        public static General LoadFromFile(string filename)
        {
            Log.Debug("Configuration", $"Loading configuration file {filename}");
            
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filename);
            General config = new General();

            config.Database.Load(data["database"]);
            
            if (data.Sections.ContainsSection("logging") == true)
                config.Logging.Load(data["logging"]);

            return config;
        }
    }
}