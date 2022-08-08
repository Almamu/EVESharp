using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.inventory;

public class TheItemIsNotYoursToTake : UserError
{
    public TheItemIsNotYoursToTake (string itemInfo) : base ("TheItemIsNotYoursToTake", new PyDictionary {["item"] = itemInfo}) { }

    public TheItemIsNotYoursToTake (int itemID) : this (itemID.ToString ()) { }
}