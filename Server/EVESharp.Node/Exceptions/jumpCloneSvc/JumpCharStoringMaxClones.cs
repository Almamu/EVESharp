using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.jumpCloneSvc
{
    public class JumpCharStoringMaxClones : UserError
    {
        public JumpCharStoringMaxClones(int have, long max) : base("JumpCharStoringMaxClones",
            new PyDictionary {["have"] = have, ["max"] = max})
        {
        }
    }
}