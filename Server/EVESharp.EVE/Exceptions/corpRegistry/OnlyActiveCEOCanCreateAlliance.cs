using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class OnlyActiveCEOCanCreateAlliance : UserError
{
    public OnlyActiveCEOCanCreateAlliance () : base ("OnlyActiveCEOCanCreateAlliance") { }
}