using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.marketProxy;

public class RepackageBeforeSelling : UserError
{
    public RepackageBeforeSelling (Type type) : base ("RepackageBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName (type.ID)}) { }
}