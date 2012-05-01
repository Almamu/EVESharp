using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CacheTool
{
    static class Message
    {
        public static void Error(string svc, string message)
        {
            WindowLog.Error(svc, message);

            MessageBox.Show(message, svc, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult Ask(string svc, string message)
        {
            WindowLog.Info(svc, message);

            return MessageBox.Show(message, svc, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static void Info(string svc, string message)
        {
            WindowLog.Debug(svc, message);

            MessageBox.Show(message, svc, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
