using System;
using IniParser.Model;

namespace ClusterControler.Configuration
{
    public class Authentication
    {
        public bool Autoaccount { get; private set; }
        public int Role { get; private set; }

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

            this.Role = int.Parse(rolestring);
        }
    }
}