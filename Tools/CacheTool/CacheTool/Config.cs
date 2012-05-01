using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CacheTool
{
    public partial class Config : Form
    {
        public Config()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindowLog.Info("Config::Save", "Saving new configuration");
            Program.ini.Write("Display", "CacheDataDisplayMode", comboBox1.Text);
            Program.cacheDataDisplayMode = comboBox1.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Config_Load(object sender, EventArgs e)
        {
            comboBox1.Text = Program.cacheDataDisplayMode;
        }
    }
}
