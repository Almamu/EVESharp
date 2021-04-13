using Node.StaticData;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.character
{
    public class BannedBloodline : UserError
    {
        public BannedBloodline(Ancestry ancestry, Bloodline bloodline) : base("BannedBloodline",
            new PyDictionary {["name"] = ancestry.Name, ["bloodlineName"] = bloodline.Name})
        {
        }
    }
}