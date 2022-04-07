using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpStationMgr;

public class RentingYouHaveAnOfficeHere : UserError
{
    public RentingYouHaveAnOfficeHere () : base ("RentingYouHaveAnOfficeHere") { }
}