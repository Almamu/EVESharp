using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.character
{
    public class BannedBloodline : UserError
    {
        public BannedBloodline(Ancestry ancestry, Bloodline bloodline) : base("BannedBloodline",
            new PyDictionary {["name"] = ancestry.Name, ["bloodlineName"] = bloodline.Name})
        {
        }
    }
}