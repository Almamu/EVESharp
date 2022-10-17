using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpAlreadyVoted : UserError
{
    public CrpAlreadyVoted () : base ("CrpAlreadyVoted") { }
}