using EVESharp.EVE.Types.Network;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.machoNet;

public class WrongMachoNode : PyException
{
    public WrongMachoNode (PyInteger nodeID) : base ("macho.WrongMachoNode", null, null, new PyDictionary() { ["payload"] = nodeID}) { }
}