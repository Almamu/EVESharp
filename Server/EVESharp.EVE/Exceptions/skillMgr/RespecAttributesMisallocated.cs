using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.skillMgr;

public class RespecAttributesMisallocated : UserError
{
    public RespecAttributesMisallocated () : base ("RespecAttributesMisallocated") { }
}