using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpStationMgr;

public class RentingYouHaveAnOfficeHere : UserError
{
    public RentingYouHaveAnOfficeHere () : base ("RentingYouHaveAnOfficeHere") { }
}