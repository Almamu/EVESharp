using System.IO;
using IniParser.Model;

namespace EVESharp.Node.Configuration
{
    public enum MachoNetMode
    {
        Single = 0,
        Server = 1,
        Proxy = 2
    };
    
    public class MachoNet
    {
        public ushort Port { get; set; }
        public MachoNetMode Mode { get; set; }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("port") == false)
                this.Port = 26000;
            else
                this.Port = ushort.Parse(section["port"]);

            string mode = (section["mode"] ?? "single").ToLower();

            this.Mode = mode switch
            {
                "proxy" => MachoNetMode.Proxy,
                "single" => MachoNetMode.Single,
                "server" => MachoNetMode.Server,
                _ => throw new InvalidDataException("Only 'proxy', 'server' or 'single' modes available")
            };
        }
    }
}