using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.marketProxy;

public class RepackageBeforeSelling : UserError
{
    public RepackageBeforeSelling (Type type) : base ("RepackageBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName (type.ID)}) { }
}