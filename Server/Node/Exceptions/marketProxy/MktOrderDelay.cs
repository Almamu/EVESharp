using System;
using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class MktOrderDelay : UserError
    {
        private static string BuildDelayString(long delay)
        {
            string result = "";
            TimeSpan timeSpan = TimeSpan.FromTicks(delay);

            if (timeSpan.Minutes > 0)
                result += $"{timeSpan.Minutes} minutes";
            if (timeSpan.Seconds > 0 && timeSpan.Minutes > 0)
                result += " and ";
            if (timeSpan.Seconds > 0)
                result += $"{timeSpan.Seconds} second(s)";
            
            return result;
        }
        
        public MktOrderDelay(long delay) : base("MktOrderDelay", new PyDictionary () {["delay"] = BuildDelayString(delay)})
        {
        }
    }
}