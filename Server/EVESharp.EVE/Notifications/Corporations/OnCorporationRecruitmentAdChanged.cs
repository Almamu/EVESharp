using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnCorporationRecruitmentAdChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationRecruitmentAdChanged";

    public  int                              CorporationID { get; init; }
    public  ulong                            AdID          { get; init; }
    private PyDictionary <PyString, PyTuple> Changes       { get; }

    public OnCorporationRecruitmentAdChanged (int corporationID, ulong adID) : base (NOTIFICATION_NAME)
    {
        this.CorporationID = corporationID;
        this.AdID          = adID;
        this.Changes       = new PyDictionary <PyString, PyTuple> ();
    }

    public OnCorporationRecruitmentAdChanged AddValue (string columnName, PyDataType oldValue, PyDataType newValue)
    {
        this.Changes [columnName] = new PyTuple (2)
        {
            [0] = oldValue,
            [1] = newValue
        };

        return this;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.CorporationID,
            this.AdID,
            this.Changes
        };
    }
}