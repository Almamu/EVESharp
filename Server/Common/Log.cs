using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common
{
    public static class Log
    {
        public static void Init( string base_filename )
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
        }

        public static void Send(ConsoleColor color, string id, string svc, string msg)
        {
            if (enabled == false)
            {
                return;
            }

            logqueue++;

            while (logqueue > 1) ;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

            Console.ForegroundColor = color;
            Console.Write(" " + id + " ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(svc + ": ");

            Console.ForegroundColor = color;
            Console.WriteLine(msg);

            Console.ForegroundColor = ConsoleColor.Gray;

            string filelog = DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString();
            filelog += " " + id + " " + svc + ": " + msg;

            FileLog(filelog);

            logqueue--;
        }

        public static void Debug(string svc, string msg)
        {
            Send(ConsoleColor.Cyan, "D", svc, msg);
        }

        public static void Error(string svc, string msg)
        {
            Send(ConsoleColor.Red, "E", svc, msg);
        }

        public static void Warning(string svc, string msg)
        {
            Send(ConsoleColor.Yellow, "W", svc, msg);
        }

        public static void Info(string svc, string msg)
        {
            Send(ConsoleColor.Green, "I", svc, msg);
        }

        public static void Trace(string svc, string msg)
        {
            Send(ConsoleColor.Gray, "T", svc, msg);
        }

        public static void Stop()
        {
            // Wait till all the logs complete
            while (logqueue > 0) ;

            enabled = false;

            fp.Close();
            fp = null;
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
    }
}
