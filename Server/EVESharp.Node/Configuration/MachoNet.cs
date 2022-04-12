using System;
using System.IO;
using IniParser.Model;

namespace EVESharp.Node.Configuration;

public enum MachoNetMode
{
    Single = 0,
    Server = 1,
    Proxy  = 2
}

public class MachoNet
{
    public ushort       Port { get; set; }
    public MachoNetMode Mode { get; set; }

    public void Load (KeyDataCollection section)
    {
        string mode = (section ["mode"] ?? "single").ToLower ();

        Mode = mode switch
        {
            "proxy"  => MachoNetMode.Proxy,
            "single" => MachoNetMode.Single,
            "server" => MachoNetMode.Server,
            _        => throw new InvalidDataException ("Only 'proxy', 'server' or 'single' modes available")
        };

        if (section.ContainsKey ("port") == false)
        {
            // determine the port based on some random data so the nodes do not collide
            if (Mode == MachoNetMode.Server)
                Port = (ushort) new Random ().Next (26001, 27000);
            else
                Port = 26000;
        }
        else
        {
            Port = ushort.Parse (section ["port"]);
        }
    }
}