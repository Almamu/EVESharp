using EVESharp.Types.Collections;
using EVESharp.Types.Serialization;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Marshaling;

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

        byte[]     data  = Marshal.ToByteArray(tuple, true);
        PyDataType final = Unmarshal.ReadFromByteArray(data);
        
        Assert.True(final == tuple);
    }
}