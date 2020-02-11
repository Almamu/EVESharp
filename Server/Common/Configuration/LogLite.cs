using IniParser.Model;

namespace Common.Configuration
{
    public class LogLite
    {
        public string Hostname { get; set; }
        public string Port { get; set; }
        public bool Enabled { get; private set; } = false;

        public void Load(KeyDataCollection section)
        {
            this.Enabled = true;
            this.Hostname = section["hostname"];
            this.Port = section["port"];
        }
    }
}