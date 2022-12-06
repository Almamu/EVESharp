using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConNotYourContract : UserError
{
    public ConNotYourContract () : base ("ConNotYourContract") { }
}