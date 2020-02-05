using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class VipKeyCommand : Decodeable
    {
        public void Decode( PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected container of type Tuple but got {data.Type}");
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 3)
            {
                throw new Exception($"Expected container to have 3 items but got {tmp.Items.Count}");
            }

            if (tmp.Items[0].Type != PyObjectType.None)
            {
                throw new Exception($"Expected item 1 to be of type None but got {tmp.Items[0].Type}");
            }

            if (tmp.Items[1].Type != PyObjectType.String)
            {
                throw new Exception($"Expected item 2 to be of type String but got {tmp.Items[1].Type}");
            }

            if (tmp.Items[2].Type != PyObjectType.String)
            {
                throw new Exception($"Expected item 3 to be of type String but got {tmp.Items[2].Type}");
            }

            PyString command = tmp.Items[1].As<PyString>();

            if (command.Value != "VK")
            {
                throw new Exception($"Expected command name to be 'VK' but got '{command.Value}'");
            }

            /* We cant check the vipKey, because the client sends different vipKeys, who know why ?
            PyString vipKey = tmp.Items[2].As<PyString>();

            if (vipKey.Value != vipkey)
            {
                Log.Error("VipKeyCommand", "Wrong vipKey value, expected \"" + vipkey + "\" but got \"" + vipKey.Value + "\"");
                return false;
            }*/
        }
    }
}
