using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class NoOfficesAreAvailableForRenting : UserError
{
    public NoOfficesAreAvailableForRenting () : base ("NoOfficesAreAvailableForRenting") { }
}