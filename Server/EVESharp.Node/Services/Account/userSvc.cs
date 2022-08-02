using EVESharp.EVE.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account;

public class userSvc : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyList GetRedeemTokens (CallInformation call)
    {
        // TODO: IMPLEMENT SUPPORT FOR USER REWARDS
        // tokens structure
        // token:
        //    tokenID
        //    massTokenID (unknown)
        //    typeID (the item that will be given to the selected character)
        //    quantity (the quantity of that item that will be given to the selected character)
        //    description (item's description)
        //    textLabel (in the case of description being absent, this would be the item's description in the UI)
        //    expireDateTime (the time the offer expires, for this to be displayed, the item's description should be in the 'textLabel' element)
        return new PyList ();
    }

    public PyDataType ClaimRedeemTokens (CallInformation call, PyList tokens, PyInteger characterID)
    {
        // TODO: IMPLEMENT SUPPORT FOR USER REWARDS
        return null;
    }

    public PyDataType ConvertETCToPilotLicence (CallInformation call, PyString code)
    {
        return null;
    }
}