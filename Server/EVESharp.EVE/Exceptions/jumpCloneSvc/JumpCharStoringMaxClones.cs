using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.jumpCloneSvc;

public class JumpCharStoringMaxClones : UserError
{
    public JumpCharStoringMaxClones (int have, long max) : base (
        "JumpCharStoringMaxClones",
        new PyDictionary
        {
            ["have"] = have,
            ["max"]  = max
        }
    ) { }
}