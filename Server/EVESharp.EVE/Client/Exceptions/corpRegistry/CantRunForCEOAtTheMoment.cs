using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CantRunForCEOAtTheMoment : UserError
{
    public CantRunForCEOAtTheMoment () : base ("CantRunForCEOAtTheMoment") { }
}