using System;
using Common;
using IniParser.Model;

namespace Common.Configuration
{
    public class Logging
    {
        public int LogLevel { get; set; }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("loglevel") == true)
            {
                string loglevels = section["loglevel"];
                
                // remove all spaces so the string is easier to split and convert everything to uppercase
                // this way the logging configuration is a bit more lenient
                loglevels = loglevels.Replace(" ", "").ToUpper();

                // separate different loglevels by the pipe character
                // to act kind of like a bitwise OR
                string[] levels = loglevels.Split('|');

                foreach (string level in levels)
                {
                    switch (level)
                    {
                        case "DEBUG":
                            LogLevel |= (int) Log.LogLevel.Debug;
                            break;
                        case "ALL":
                            LogLevel |= (int) Log.LogLevel.All;
                            break;
                        case "TRACE":
                            LogLevel |= (int) Log.LogLevel.Trace;
                            break;
                        case "ERROR":
                            LogLevel |= (int) Log.LogLevel.Error;
                            break;
                        case "INFO":
                            LogLevel |= (int) Log.LogLevel.Info;
                            break;
                        case "WARN":
                        case "WARNING":
                            LogLevel |= (int) Log.LogLevel.Warning;
                            break;
                        default:
                            throw new Exception(String.Format("Unknown logging level {0}", level));
                    }
                }
                
                // warn the user if some basic levels are not enabled
                if ((LogLevel & ((int) Log.LogLevel.Error)) == 0)
                {
                    Log.Error("Configuration", "-------------------------------------------------");
                    Log.Error("Configuration", "Error output is disabled. This is not recommended");
                    Log.Error("Configuration", "-------------------------------------------------");
                }
            }
        }
    }
}