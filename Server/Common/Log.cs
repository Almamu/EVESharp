using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Common
{
    public static class Log
    {
        public enum LogLevel
        {
            None = 0,
            Debug = 1,
            Error = 2,
            Warning = 4,
            Info = 8,
            Trace = 16,
            All = 31,
        }

        public static void Init(string base_filename, int level)
        {
            if (Directory.Exists("logs") == false)
            {
                Directory.CreateDirectory("logs");
            }

            file = "logs/" + base_filename.Replace('/', '_') + "_";

            file += DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_";
            file += DateTime.Now.Hour + "h " + DateTime.Now.Minute + "m " + DateTime.Now.Second + "s.log";

            fp = File.OpenWrite(file);
            enabled = true;

            SetLogLevel(level);
        }

        public static void SetLogLevel(int level)
        {
            logLevel = level;
        }

        public static void Init(string base_filename)
        {
            Init(base_filename, (int) LogLevel.All);
        }

        public static void Send(LogLevel level, ConsoleColor color, string id, string svc, string msg)
        {
            if (enabled == false)
            {
                return;
            }

            // Check for undesired message types
            if ((((int) level) & logLevel) == 0)
            {
                return;
            }

            while (logqueue > 0) Thread.Sleep(1);

            logqueue++;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

            Console.ForegroundColor = color;
            Console.Write(" " + id + " ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(svc + ": ");

            Console.ForegroundColor = color;
            Console.WriteLine(msg);

            Console.ForegroundColor = ConsoleColor.Gray;

            string filelog = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            filelog += " " + id + " " + svc + ": " + msg;

            FileLog(filelog);

            logqueue--;
        }

        public static void Debug(string svc, string msg)
        {
            Send(LogLevel.Debug, ConsoleColor.Cyan, "D", svc, msg);
        }

        public static void Error(string svc, string msg)
        {
            Send(LogLevel.Error, ConsoleColor.Red, "E", svc, msg);
        }

        public static void Warning(string svc, string msg)
        {
            Send(LogLevel.Warning, ConsoleColor.Yellow, "W", svc, msg);
        }

        public static void Info(string svc, string msg)
        {
            Send(LogLevel.Info, ConsoleColor.Green, "I", svc, msg);
        }

        public static void Trace(string svc, string msg)
        {
            Send(LogLevel.Trace, ConsoleColor.Gray, "T", svc, msg);
        }

        public static void Stop()
        {
            // Wait till all the logs complete
            enabled = false;

            while (logqueue > 0) ;

            fp.Close();
            fp = null;

            logLevel = (int) LogLevel.None;
        }

        public static void FileLog(string line)
        {
            if (fp == null)
            {
                return;
            }

            byte[] data = Encoding.ASCII.GetBytes(line + "\r\n");
            fp.Write(data, 0, data.Length);
            fp.Flush();
        }

        public static bool enabled = false;
        public static int logqueue = 0;
        public static string file = "";
        private static FileStream fp = null;
        private static int logLevel = (int) LogLevel.None;
    }
}
