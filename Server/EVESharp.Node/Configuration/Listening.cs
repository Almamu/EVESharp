using IniParser.Model;

namespace EVESharp.Node.Configuration
{
    public class Listening
    {
        public ushort Port { get; set; }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("port") == false)
                this.Port = 26000;
            else
                this.Port = ushort.Parse(section["port"]);
        }
    }
}