using System;
using System.IO;
using Common;

namespace EVESharp.Configuration
{
    public class General
    {
        private Database m_Database = new Database ();

        public Database Database
        {
            get { return this.m_Database; }
            set { this.m_Database = value; }
        }

        public static General LoadFromFile(string filename)
        {
            Log.Debug("Configuration", String.Format("Loading configuration file {0}", filename));
            
            string[] lines = File.ReadAllLines(filename);
            General config = new General();

            config.Database.Username = lines[0];
            config.Database.Password = lines[1];
            config.Database.Hostname = lines[2];
            config.Database.Name = lines[3];
            
            return config;
        }
    }
}