using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.character;

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