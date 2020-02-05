using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    public class PlaceboRequest : Decodeable
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

            if (tmp.Items[0].Type != PyObjectType.String)
            {
                throw new Exception($"Expected item 1 to be of type string but got {tmp.Items[0].Type}");
            }

            if (tmp.Items[1].Type != PyObjectType.Dict)
            {
                throw new Exception($"Expected item 2 to be of type PyDict but got {tmp.Items[1].Type}");
            }

            PyString command = tmp.Items[0].As<PyString>();

            if (command.Value != "placebo")
            {
                throw new Exception($"Expected command type of 'placebo' but got {command.Value}");
            }

            PyDict args = tmp.Items[1].As<PyDict>();

            if (args.Dictionary.Count != 0)
            {
                throw new Exception("Arguments not supported yet");
            }
        }
    }
}
