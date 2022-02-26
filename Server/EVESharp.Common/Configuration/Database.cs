using IniParser.Model;

namespace EVESharp.Common.Configuration
{
    public class Database
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Hostname { get; private set; }
        public string Name { get; private set; }
        public uint Port { get; private set; }

        public void Load(KeyDataCollection section)
        {
            this.Username = section["username"];
            this.Password = section["password"];
            this.Hostname = section["hostname"];
            this.Name = section["name"];
            this.Port = uint.Parse(section["port"] ?? "3306");
        }
    }
}