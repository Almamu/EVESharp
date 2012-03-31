using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class VipKeyCommand
    {
        public string vipkey = "5091";

        public bool Decode( PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                Log.Error("VipKeyCommand", "Wrong type");
                return false;
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 3)
            {
                Log.Error("VipKeyCommand", "Wrong size, expected 3 but got " + tmp.Items.Count);
                return false;
            }

            if (tmp.Items[0].Type != PyObjectType.None)
            {
                Log.Error("VipKeyCommand", "Wrong type for item 1");
                return false;
            }

            if (tmp.Items[1].Type != PyObjectType.String)
            {
                Log.Error("VipKeyCommand", "Wrong type for item 2");
                return false;
            }

            if (tmp.Items[2].Type != PyObjectType.String)
            {
                Log.Error("VipKeyCommand", "Wrong type for item 3");
                return false;
            }

            PyString command = tmp.Items[1].As<PyString>();

            if (command.Value != "VK")
            {
                Log.Error("VipKeyCommand", "Wrong command name, expected VK but got \"" + command.Value + "\"");
                return false;
            }

            PyString vipKey = tmp.Items[2].As<PyString>();

            if (vipKey.Value != vipkey)
            {
                Log.Error("VipKeyCommand", "Wrong vipKey value, expected \"" + vipkey + "\" but got \"" + vipKey.Value + "\"");
                return false;
            }

            return true;
        }
    }
}
