using System.Collections.Generic;
using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Unit.Marshaling;
using EVESharp.PythonTypes.Unit.Marshaling.MarshalData;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Types.Primitives;

public class PyPackedRowTests
{
    [Test]
    public void PackedRowComparison()
    {
        IEnumerator<byte[]> marshal = new PackedRowMarshalData().GetEnumerator();
        IEnumerator<PyPackedRow> rows = PackedRowMarshallingTests.GetRows().GetEnumerator();
        
        // take three out from both
        PyPackedRow[] marshaled = new PyPackedRow[3];
        PyPackedRow[] original = new PyPackedRow[3];
        PyPackedRow nullRow = null;

        for (int i = 0; i < marshaled.Length; i++)
        {
            marshal.MoveNext();
            rows.MoveNext();
            
            marshaled[i] = Unmarshal.ReadFromByteArray(marshal.Current) as PyPackedRow;
            original[i] = rows.Current;
        }

        for (int i = 0; i < marshaled.Length; i++)
        {
            Assert.True(original[i] == marshaled[i]);
            Assert.False(original[i] != marshaled[i]);
        }
        
        Assert.True(original[0] != marshaled[1]);
        Assert.False(original[0] == marshaled[1]);
        
        Assert.False(original[0] == null);
        Assert.True(original[0] != null);
        Assert.False(original[0] is null);
        Assert.True(original[0] is not null);
        Assert.True(nullRow == null);
        Assert.False(nullRow != null);
        Assert.True(nullRow is null);
        Assert.False(nullRow is not null);
        Assert.False(original[0] == nullRow);
        Assert.True(original[0] != nullRow);
    }
}