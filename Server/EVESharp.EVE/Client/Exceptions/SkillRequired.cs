using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions;

public class SkillRequired : UserError
{
    public SkillRequired (Types skill) : this ((int) skill) { }

    public SkillRequired (int skill) : base (
        "SkillRequired",
        new PyDictionary {["skillName"] = FormatTypeIDAsName (skill)}
    ) { }
}