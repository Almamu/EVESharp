using IniParser.Model;

namespace EVESharp.Common.Configuration
{
    public class FileLog
    {
        public string Directory { get; private set; }
        public string LogFile { get; private set; }

        public bool Enabled { get; private set; } = false;

        public void Load(KeyDataCollection section)
        {
            this.Enabled = true;
            this.Directory = section["directory"];
            this.LogFile = section["logfile"];
        }
    }
}