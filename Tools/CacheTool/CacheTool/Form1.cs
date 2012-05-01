using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Common;
using Marshal;
using Common.Packets;
using CacheTool.Database;

namespace CacheTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> cacheNames = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowLog.Init(this);
            WindowLog.Debug("Main::Load", "Loading settings from INI file");
            
            Program.CreateIniFile();

            Program.cacheDataDisplayMode = Program.ini.Read("Display", "CacheDataDisplayMode");

            if (Database.Database.Init() == false)
            {
                WindowLog.Error("Main::Load", "Cannot connect to database");
                Application.Exit();
            }

            WindowLog.Debug("Main::Load", "Loading cache names for common cache");

            // Load the cache list for common caches
            cacheNames = CacheDB.GetCommonCacheNames();

            foreach (string name in cacheNames)
            {
                WindowLog.Debug("Main::Load", "Adding cache " + name + " to cache list");
                listBox1.Items.Add(name);
            }

            WindowLog.Debug("Main::Load", "Application started");
        }

        public void WriteLog(string message)
        {
            richTextBox1.Text += message + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length + 1;
            richTextBox1.ScrollToCaret();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            WindowLog.Debug("Main::Close", "Disconnecting from the database");
            Database.Database.Stop();
            WindowLog.Stop();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowLog.Warning("Main::ToolStrip", "User requested close");
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                richTextBox2.Text = "";
                return;
            }

            if (Cache.UpdateCache(listBox1.SelectedItem.ToString()) == false)
            {
                WindowLog.Error("Main::LoadCache", "Error loading cache");
                return;
            }

            PyObject cache = Cache.GetCache(listBox1.SelectedItem.ToString());

            PyCachedObject obj = new PyCachedObject();

            if (obj.Decode(cache) == false)
            {
                WindowLog.Error("Main::LoadCache", "Cannot decode the cache data");
                return;
            }

            if (Program.cacheDataDisplayMode == "Pretty")
            {
                try
                {
                    richTextBox2.Text = PrettyPrinter.Print(Unmarshal.Process<PyObject>(obj.cache.Data));
                }
                catch (Exception)
                {
                    WindowLog.Error("Main::LoadCache", "Cannot Unmarshal the cache data");
                    richTextBox2.Text = "Error";
                }
            }
            else
            {
                richTextBox2.Text = ByteToString(obj.cache.Data);
            }

            WindowLog.Debug("Main::LoadCache", "Cache loaded");
        }

        private string ByteToString(byte[] input)
        {
            string result = "";

            foreach (byte v in input)
            {
                string current = new string((char)v, 1);
                
                if (v == '\0')
                {
                    current = "\\0";
                }
                else if (v == '\n')
                {
                    current = "\\n";
                }
                else if (v == '\t')
                {
                    current = "\\t";
                }
                else if (v == '\b')
                {
                    current = "\\b";
                }
                else if (v == '\r')
                {
                    current = "\\r";
                }

                result += current;
            }

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                Message.Error("Main::ChangeCacheName", "Debes de introducir el nuevo nombre para la cache");
                return;
            }

            if (listBox1.SelectedItem == null)
            {
                Message.Error("Main::ChangeCacheName", "Debes elegir una cache a la que cambiarle el nombre");
                return;
            }

            if (Cache.UpdateCacheName(listBox1.SelectedItem.ToString(), textBox1.Text) == false)
            {
                Message.Error("Main::ChangeCacheName", "No se pudo cambiar el nombre a la cache, error desconocido");
            }

            cacheNames[cacheNames.IndexOf(listBox1.SelectedItem.ToString())] = textBox1.Text;
            listBox1.Items.Remove(listBox1.SelectedItem);
            listBox1.SelectedIndex = listBox1.Items.Add(textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                Message.Error("Main::DeleteCache", "Debes de seleccionar la cache a borrar");
                return;
            }

            Cache.DeleteCache(listBox1.SelectedItem.ToString());
            cacheNames.Remove(listBox1.SelectedItem.ToString());
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                Message.Error("Main::CreateCache", "Debes introducir el nombre de la caché");
                return;
            }

            Cache.SaveCacheFor(textBox1.Text, new PyNone(), DateTime.Now.ToFileTime(), 0xFFAA);
            listBox1.Items.Add(textBox1.Text);
            cacheNames.Add(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                Message.Error("Main::LoadFile", "Debes elejir la cache a sobreescribir");
                return;
            }

            openFileDialog1.Filter = "Todos los archivos(*.*)|*.*";
            openFileDialog1.Multiselect = false;
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(openFileDialog1.FileName) == false)
                {
                    Message.Error("Main::LoadFile", "El archivo elejido no existe");
                    button3_Click(sender, e);
                }

                FileStream fp = File.OpenRead(openFileDialog1.FileName);

                byte[] data = fp.ReadAllBytes();

                Cache.SaveCacheFor(listBox1.SelectedItem.ToString(), data, DateTime.Now.ToFileTime(), 0xFFAA);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                Message.Error("Main::SaveFile", "Debes elejir la cache a guardar");
                return;
            }

            saveFileDialog1.Filter = "Archivos de cache(*.cache)|*.cache";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            PyObject cache = Cache.GetCache(listBox1.SelectedItem.ToString());

            PyCachedObject obj = new PyCachedObject();

            if (obj.Decode(cache) == false)
            {
                Message.Error("Main::SaveFile", "No se pudo obtener la información correctamente");
                return;
            }

            if (File.Exists(saveFileDialog1.FileName) == true)
            {
                try
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                catch (Exception)
                {
                
                }
            }

            FileStream fp = File.OpenWrite(saveFileDialog1.FileName);

            fp.Write(obj.cache.Data, 0, obj.cache.Data.Length);

            fp.Close();

            WindowLog.Debug("Main::SaveFile", "Cache saved sucessful");
        }

        private void toolStripDropDownButton2_Click(object sender, EventArgs e)
        {
            Config frm = new Config();
            this.Enabled = false;
            frm.ShowDialog();
            this.Enabled = true;
        }
    }
}
