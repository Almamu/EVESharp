using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpStationMgr;

public class NoOfficesAreAvailableForRenting : UserError
{
    public NoOfficesAreAvailableForRenting () : base ("NoOfficesAreAvailableForRenting") { }
}