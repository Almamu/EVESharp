using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class QueueCheckCommand
    {
        public bool Decode(PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                Log.Error("QueueCheckCommand", "Wrong type");
                return false;
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 2)
            {
                Log.Error("QueueCheckCommand", "Wrong size, expected 2 but got " + tmp.Items.Count);
                return false;
            }

            if (tmp.Items[0].Type != PyObjectType.None)
            {
                Log.Error("QueueCheckCommand", "Wrong type for item 1");
                return false;
            }

            if (tmp.Items[1].Type != PyObjectType.String)
            {
                Log.Error("QueueCheckCommand", "Wrong type for item 2");
                return false;
            }

            PyString command = tmp.Items[1].As<PyString>();

            if (command.Value != "QC")
            {
                Log.Error("QueueCheckCommand", "Wrong value for command, expected \"QC\" but got \"" + command.Value + "\"");
                return false;
            }

            return true;
        }
    }
}
