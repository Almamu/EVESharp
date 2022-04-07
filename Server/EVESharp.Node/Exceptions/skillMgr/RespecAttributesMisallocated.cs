using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.skillMgr;

public class RespecAttributesMisallocated : UserError
{
    public RespecAttributesMisallocated () : base ("RespecAttributesMisallocated") { }
}