using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CacheTool
{
    static class Program
    {
        static public INIFile ini = new INIFile("./config.ini");
        static public string cacheDataDisplayMode = "Raw";

        /// <summary>
        /// Creates the INI config file with default values
        /// </summary>
        static public void CreateIniFile()
        {
            if (File.Exists("config.ini") == true)
            {
                return;
            }

            ini = new INIFile("./config.ini");
            File.Create("config.ini").Close();
            ini.Write("Display", "CacheDataDisplayMode", "Pretty");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
