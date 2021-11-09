using IniParser.Model;

namespace EVESharp.Proxy.Configuration
{
    public class Listening
    {
        public ushort Port { get; private set; }

        public Listening()
        {
            this.Port = 26000;
        }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("port") == false)
                return;

            Port = ushort.Parse(section["port"]);
        }
    }
}