using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.skillMgr;

public class RespecAttributesMisallocated : UserError
{
    public RespecAttributesMisallocated () : base ("RespecAttributesMisallocated") { }
}