using System.Collections.Generic;
using EVESharp.EVE.Packets;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.EVE.Unit.Packets;

public class ClientCommandTests
{
    private const string COMMAND = "QC";
    
    [Test]
    public void ClientCommandBuild ()
    {
        PyDataType data = new ClientCommand (COMMAND);

        PyTuple tuple = PyAssert.Tuple (data, 3);
        PyAssert.String (tuple [1], COMMAND);
    }

    [Test]
    public void ClientCommandParse ()
    {
        ClientCommand command = new PyTuple (3)
        {
            [0] = null,
            [1] = COMMAND,
            [2] = null
        };

        Assert.AreEqual (command.Command, COMMAND);
    }
}