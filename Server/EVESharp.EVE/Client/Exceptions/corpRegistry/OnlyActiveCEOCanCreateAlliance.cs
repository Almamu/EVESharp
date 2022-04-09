using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class OnlyActiveCEOCanCreateAlliance : UserError
{
    public OnlyActiveCEOCanCreateAlliance () : base ("OnlyActiveCEOCanCreateAlliance") { }
}