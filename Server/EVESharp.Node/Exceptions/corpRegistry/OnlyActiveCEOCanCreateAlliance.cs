using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class OnlyActiveCEOCanCreateAlliance : UserError
{
    public OnlyActiveCEOCanCreateAlliance () : base ("OnlyActiveCEOCanCreateAlliance") { }
}