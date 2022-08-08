using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.skillMgr;

public class RespecAttributesTooLow : UserError
{
    public RespecAttributesTooLow () : base ("RespecAttributesTooLow") { }
}