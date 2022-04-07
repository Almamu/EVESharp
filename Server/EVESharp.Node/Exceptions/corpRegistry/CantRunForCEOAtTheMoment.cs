using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CantRunForCEOAtTheMoment : UserError
{
    public CantRunForCEOAtTheMoment () : base ("CantRunForCEOAtTheMoment") { }
}