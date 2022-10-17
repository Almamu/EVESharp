using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnSanctionedActionChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnSanctionedActionChanged";
    
    public int                              CorporationID { get; }
    public int                              VoteCaseID    { get; }
    public PyDictionary <PyString, PyTuple> Changes       { get; }

    public OnSanctionedActionChanged (int corporationID, int voteCaseID, PyDictionary<PyString, PyTuple> changes = null) : base (NOTIFICATION_NAME)
    {
        this.CorporationID = corporationID;
        this.VoteCaseID    = voteCaseID;
        this.Changes       = changes ?? new PyDictionary <PyString, PyTuple> ();
    }
    
    public OnSanctionedActionChanged AddValue (string columnName, PyDataType oldValue, PyDataType newValue)
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
            this.VoteCaseID,
            this.Changes
        };
    }
}