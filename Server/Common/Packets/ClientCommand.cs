using System.IO;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class ClientCommand
    {
        public string Command { get; set; }

        public ClientCommand(string command)
        {
            this.Command = command;
        }

        public static implicit operator ClientCommand(PyDataType data)
        {
            PyTuple tuple = data as PyTuple;

            if (tuple.Count != 2 && tuple.Count != 3)
                throw new InvalidDataException($"Expected a tuple of two or three elements");

            return new ClientCommand(tuple[1] as PyString);
        }
    }
}