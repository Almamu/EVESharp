using System;
using EVESharp.Common.Constants;
using EVESharp.EVE;
using IniParser.Model;

namespace EVESharp.Proxy.Configuration
{
    public class Authentication
    {
        public bool Autoaccount { get; private set; }
        public long Role { get; private set; }

        public Authentication()
        {
            this.Autoaccount = false;
        }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("enabled") == false)
                return;

            string enablestring = section["enabled"].ToUpper();

            this.Autoaccount = enablestring == "YES" || enablestring == "1" || enablestring == "TRUE";

            if (section.ContainsKey("role") == false)
                throw new Exception("With autoaccount enabled you MUST specify a default role");

            string rolestring = section["role"];
            string[] rolelist = rolestring.Split(",");

            foreach (string role in rolelist)
            {
                string trimedRole = role.Trim();

                // ignore empty roles
                if (trimedRole == "")
                    continue;
                
                if (Roles.TryParse(trimedRole, out Roles roleValue) == false)
                    throw new Exception($"Unknown role value {role.Trim()}");

                this.Role |= (long) roleValue;
            }
        }
    }
}