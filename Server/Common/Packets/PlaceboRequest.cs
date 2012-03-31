using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class PlaceboRequest
    {
        public string request = "placebo";

        // We dont need to encode it, just decode as this is only sent by the client
        public bool Decode(PyObject data)
        {
            if (data.Type != PyObjectType.Tuple)
            {
                Log.Error("PlaceboRequest", "Wrong type");
                return false;
            }

            PyTuple tmp = data.As<PyTuple>();

            if (tmp.Items.Count != 2)
            {
                Log.Error("PlaceboRequest", "Wrong item count, expected 2 but got " + tmp.Items.Count);
                return false;
            }

            if (tmp.Items[0].Type != PyObjectType.String)
            {
                Log.Error("PlaceboRequest", "Wrong item 1 type");
                return false;
            }

            if (tmp.Items[1].Type != PyObjectType.Dict)
            {
                Log.Error("PlaceboRequest", "Wrong item 2 type");
                return false;
            }

            PyString command = tmp.Items[0].As<PyString>();

            if (command.Value != "placebo")
            {
                Log.Error("PlaceboRequest", "Wrong value for command, expected \"" + request + "\", but got \"" + command.Value + "\"");
                return false;
            }

            PyDict args = tmp.Items[1].As<PyDict>();

            if (args.Dictionary.Count != 0)
            {
                Log.Warning("PlaceboRequest", "PlaceboRequest arguments are not supported yet");
                Log.Warning("PlaceboRequest", PrettyPrinter.Print(args));
            }

            return true;
        }
    }
}
