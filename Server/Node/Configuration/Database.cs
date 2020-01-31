using IniParser.Model;

namespace EVESharp.Configuration
{
    public class Database
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
        public string Name { get; set; }

        public void Load(KeyDataCollection section)
        {
            this.Username = section["username"];
            this.Password = section["password"];
            this.Hostname = section["hostname"];
            this.Name = section["name"];
        }
    }
}