using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class QueueCheckCommand : Decodeable
    {
        public void Decode(PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected container of type Tuple but got {data.Type}");
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 2)
            {
                throw new Exception($"Expected container to have 2 items but got {tmp.Items.Count}");
            }

            if (tmp.Items[0].Type != PyObjectType.None)
            {
                throw new Exception($"Expected item 1 to be of type None but got {tmp.Items[0].Type}");
            }

            if (tmp.Items[1].Type != PyObjectType.String)
            {
                throw new Exception($"Expected item 2 to be of type String but got {tmp.Items[1].Type}");
            }

            PyString command = tmp.Items[1].As<PyString>();

            if (command.Value != "QC")
            {
                throw new Exception($"Expected command 'QC' but got '{command.Value}'");
            }
        }
    }
}
