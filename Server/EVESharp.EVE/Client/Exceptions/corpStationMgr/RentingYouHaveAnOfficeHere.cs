using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpStationMgr;

public class RentingYouHaveAnOfficeHere : UserError
{
    public RentingYouHaveAnOfficeHere () : base ("RentingYouHaveAnOfficeHere") { }
}