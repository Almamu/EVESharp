using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.marketProxy;

public class RepackageBeforeSelling : UserError
{
    public RepackageBeforeSelling (Type type) : base ("RepackageBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName (type.ID)}) { }
}