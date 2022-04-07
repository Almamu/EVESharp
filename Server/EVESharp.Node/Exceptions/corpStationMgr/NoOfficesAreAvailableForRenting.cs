using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpStationMgr;

public class NoOfficesAreAvailableForRenting : UserError
{
    public NoOfficesAreAvailableForRenting () : base ("NoOfficesAreAvailableForRenting") { }
}