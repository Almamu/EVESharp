﻿using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Corporations;

public class OnCorporationRecruitmentAdChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationRecruitmentAdChanged";

    public  int                              CorporationID { get; init; }
    public  ulong                            AdID          { get; init; }
    private PyDictionary <PyString, PyTuple> Changes       { get; }

    public OnCorporationRecruitmentAdChanged (int corporationID, ulong adID) : base (NOTIFICATION_NAME)
    {
        CorporationID = corporationID;
        AdID          = adID;
        Changes       = new PyDictionary <PyString, PyTuple> ();
    }

    public OnCorporationRecruitmentAdChanged AddValue (string columnName, PyDataType oldValue, PyDataType newValue)
    {
        Changes [columnName] = new PyTuple (2)
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
            CorporationID,
            AdID,
            Changes
        };
    }
}