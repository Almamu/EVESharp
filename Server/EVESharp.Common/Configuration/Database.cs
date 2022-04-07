using IniParser.Model;

namespace EVESharp.Common.Configuration;

public class Database
{
    public string Username { get; private set; }
    public string Password { get; private set; }
    public string Hostname { get; private set; }
    public string Name     { get; private set; }

    public void Load (KeyDataCollection section)
    {
        Username = section ["username"];
        Password = section ["password"];
        Hostname = section ["hostname"];
        Name     = section ["name"];
    }
}