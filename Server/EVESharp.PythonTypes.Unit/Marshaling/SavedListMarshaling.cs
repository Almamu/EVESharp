using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Marshaling;

public class SavedListMarshaling
{
    [Test]
    public void SavedMarshalingTest()
    {
        PyList list = new PyList() { 5000000, 15000000, 150.0 };
        PyTuple tuple = new PyTuple(3)
        {
            [0] = list,
            [1] = list,
            [2] = list
        };

        byte[] data = Marshal.Marshal.ToByteArray(tuple, true);
        PyDataType final = Unmarshal.ReadFromByteArray(data);
        
        Assert.True(final == tuple);
    }
}