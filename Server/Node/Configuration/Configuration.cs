using System;
using System.IO;
using Common;
using Common.Configuration;
using IniParser;
using IniParser.Model;

namespace Node.Configuration
{
    public class General
    {
        private Common.Configuration.Database m_Database = new Common.Configuration.Database ();

        private Logging m_Logging = new Logging ();

        private Proxy m_Proxy = new Proxy();
        
        private Authentication m_Authentication = new Authentication();

        public Common.Configuration.Database Database
        {
            get { return this.m_Database; }
            set { this.m_Database = value; }
        }

        public Logging Logging
        {
            get { return this.m_Logging; }
            set { this.m_Logging = value; }
        }

        public Proxy Proxy
        {
            get { return this.m_Proxy; }
            set { this.m_Proxy = value; }
        }

        public Authentication Authentication
        {
            get { return this.m_Authentication; }
            set { this.m_Authentication = value; }
        }
        
        public static General LoadFromFile(string filename)
        {
            Log.Debug("Configuration", String.Format("Loading configuration file {0}", filename));
            
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filename);
            General config = new General();

            config.Database.Load(data["database"]);
            config.Proxy.Load(data["proxy"]);
            
            if (data.Sections.ContainsSection("logging") == true)
                config.Logging.Load(data["logging"]);

            if(data.Sections.ContainsSection("authentication") == true)
                config.Authentication.Load(data["authentication"]);
            
            return config;
        }
    }
}