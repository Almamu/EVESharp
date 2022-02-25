using System;
using IniParser.Model;

namespace EVESharp.Node.Configuration;

public class Cluster
{
    public string OrchestatorURL { get; private set; }
    
    public void Load(KeyDataCollection section)
    {
        if (section.ContainsKey("url") == false)
            this.OrchestatorURL = "";
        else
            this.OrchestatorURL = section["url"];
    }
}