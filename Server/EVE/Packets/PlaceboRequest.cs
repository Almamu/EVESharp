using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets
{
    public class PlaceboRequest
    {
        public PyString Command { get; }
        public PyDictionary Arguments { get; }

        public PlaceboRequest(PyString command, PyDictionary arguments)
        {
            this.Command = command;
            this.Arguments = arguments;
        }

        public static implicit operator PlaceboRequest(PyDataType request)
        {
            PyTuple data = request as PyTuple;

            if (data.Count != 2)
                throw new InvalidDataException($"Expected tuple of two items");

            return new PlaceboRequest(
                data[0] as PyString,
                data[1] as PyDictionary
            );
        }
    }
}