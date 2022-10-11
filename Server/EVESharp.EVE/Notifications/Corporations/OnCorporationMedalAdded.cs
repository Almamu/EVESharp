using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnCorporationMedalAdded : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationMedalAdded";
    
    public int MedalID { get; init; }

    public OnCorporationMedalAdded (int medalID) : base (NOTIFICATION_NAME)
    {
        this.MedalID = medalID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> ()
        {
            this.MedalID
        };
    }
}