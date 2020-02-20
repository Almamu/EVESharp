using System.Collections.Generic;
using IniParser.Model;

namespace Common.Configuration
{
    public class Logging
    {
        public List<string> EnableChannels = new List<string>();

        public void Load(KeyDataCollection collection)
        {
            if (collection.ContainsKey("force") == false)
                return;

            // load suppressed channels from the line
            string[] channels = collection["force"].Split(',');

            this.EnableChannels.AddRange(channels);
        }
    }
}