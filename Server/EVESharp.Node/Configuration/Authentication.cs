using System;
using EVESharp.EVE.Account;
using IniParser.Model;

namespace EVESharp.Node.Configuration;

public enum AuthenticationMessageType
{
    NoMessage   = 0,
    HTMLMessage = 1
}

public class Authentication
{
    public AuthenticationMessageType MessageType { get; private set; }
    public string                    Message     { get; private set; }
    public bool                      Autoaccount { get; private set; }
    public long                      Role        { get; private set; }

    public void Load (KeyDataCollection section)
    {
        if (section.ContainsKey ("loginMessageType") == false)
        {
            MessageType = AuthenticationMessageType.NoMessage;

            return;
        }

        string value = section ["loginMessageType"].ToUpper ();

        switch (value)
        {
            case "MESSAGE":
                if (section.ContainsKey ("loginMessage") == false)
                    throw new Exception ("Authentication service configuration must specify an HTML message");

                Message     = section ["loginMessage"];
                MessageType = AuthenticationMessageType.HTMLMessage;
                break;

            case "NONE":
            default:
                MessageType = AuthenticationMessageType.NoMessage;
                break;

        }

        if (section.ContainsKey ("autoaccount") == false)
            return;

        string enablestring = section ["autoaccount"].ToUpper ();

        Autoaccount = enablestring == "YES" || enablestring == "1" || enablestring == "TRUE";

        if (section.ContainsKey ("role") == false)
            throw new Exception ("With autoaccount enabled you MUST specify a default role");

        string    rolestring = section ["role"];
        string [] rolelist   = rolestring.Split (",");

        foreach (string role in rolelist)
        {
            string trimedRole = role.Trim ();

            // ignore empty roles
            if (trimedRole == "")
                continue;

            if (Enum.TryParse (trimedRole, out Roles roleValue) == false)
                throw new Exception ($"Unknown role value {role.Trim ()}");

            Role |= (long) roleValue;
        }
    }
}