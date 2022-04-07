using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.skillMgr;

public class RespecAttributesTooLow : UserError
{
    public RespecAttributesTooLow () : base ("RespecAttributesTooLow") { }
}