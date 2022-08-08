using EVESharp.EVE.Data;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.character;

public class BannedBloodline : UserError
{
    public BannedBloodline (Ancestry ancestry, Bloodline bloodline) : base (
        "BannedBloodline",
        new PyDictionary
        {
            ["name"]          = ancestry.Name,
            ["bloodlineName"] = bloodline.Name
        }
    ) { }
}