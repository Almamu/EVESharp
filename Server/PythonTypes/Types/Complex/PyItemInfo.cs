using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    public class PyItemInfo : IndexRowset
    {
        public PyItemInfo() : base("itemID", new string[]
        {
            "itemID", "invItem", "activeEffects", "attributes", "time"
        })
        {
        }

        public void AddRow(int itemID, PyPackedRow entityRow, PyDictionary effects, PyDictionary attributes, long time)
        {
            this.AddRow(itemID, new PyList(new PyDataType[]
            {
                itemID, entityRow, effects, attributes, time
            }));
        }
    }
}