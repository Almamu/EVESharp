using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.skillMgr;

public class RespecAttributesTooHigh : UserError
{
    public RespecAttributesTooHigh () : base ("RespecAttributesTooHigh") { }
}