using System.Runtime.CompilerServices;
using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;

namespace Node.Exceptions
{
    public class SkillRequired : UserError
    {
        public SkillRequired(Types skill) : this((int) skill)
        {
        }
        
        public SkillRequired(int skill) : base("SkillRequired", 
            new PyDictionary {["skillName"] = FormatTypeIDAsName(skill)})
        {
        }
    }
}