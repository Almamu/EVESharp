using System;
using Common.Constants;
using IniParser.Model;

namespace ClusterControler.Configuration
{
    public class Authentication
    {
        public bool Autoaccount { get; private set; }
        public Roles Role { get; private set; }

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
            
            if(section.ContainsKey("role") == false)
                throw new Exception("With autoaccount enabled you MUST specify a default role");

            string rolestring = section["role"];
            Roles role;

            if(Roles.TryParse(rolestring, out role) == false)
                throw new Exception($"Unknown role value {rolestring}");

            this.Role = role;
        }
    }
}