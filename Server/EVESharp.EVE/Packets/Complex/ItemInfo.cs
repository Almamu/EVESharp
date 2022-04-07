using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets.Complex;

/// <summary>
/// Simple base class to simplify working with Item information
/// </summary>
public class ItemInfo : IndexRowset
{
    public ItemInfo() : base("itemID", new PyList<PyString>(5)
    {
        [0] = "itemID",
        [1] = "invItem",
        [2] = "activeEffects",
        [3] = "attributes",
        [4] = "time"
    })
    {
    }

    public void AddRow(int itemID, PyPackedRow entityRow, PyDictionary effects, PyDictionary attributes, long time)
    {
        this.AddRow(itemID, new PyList(5)
        {
            [0] = itemID,
            [1] = entityRow,
            [2] = effects,
            [3] = attributes,
            [4] = time
        });
    }
}