using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace CacheTool
{
    static class WindowLog
    {
        public static void Init(Form1 from)
        {
            Log.Init("cachetool");

            enabled = true;

            output = from;
        }

        private static void Print(string type, string svc, string message)
        {
            if (enabled == false)
            {
                return;
            }

            while (queue > 0) ;

            queue++;

            string msg = type + " " + svc + ": " + message;
            output.WriteLog(msg);

            queue--;
        }

        public static void Error(string svc, string message)
        {
            Print("ERROR", svc, message);
            Log.Error(svc, message);
        }

        public static void Debug(string svc, string message)
        {
            Print("DEBUG", svc, message);
            Log.Debug(svc, message);
        }

        public static void Warning(string svc, string message)
        {
            Print("WARNING", svc, message);
            Log.Warning(svc, message);
        }

        public static void Trace(string svc, string message)
        {
            Print("TRACE", svc, message);
            Log.Trace(svc, message);
        }

        public static void Info(string svc, string message)
        {
            Print("INFO", svc, message);
            Log.Info(svc, message);
        }

        public static void Stop()
        {
            enabled = false;

            // Wair for current logs to end
            while (queue > 0) ;

            output = null;
        }

        private static int queue = 0;
        private static bool enabled = false;
        private static Form1 output = null;
    }
}
