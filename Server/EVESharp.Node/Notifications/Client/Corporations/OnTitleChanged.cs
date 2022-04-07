using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Corporations;

public class OnTitleChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnTitleChanged";
        
    public int                             CorporationID { get; init; }
    public int                             TitleID       { get; init; }
    public PyDictionary<PyString, PyTuple> Changes       { get; init; }
            
    public OnTitleChanged(int corporationID, int titleID) : base(NOTIFICATION_NAME)
    {
        this.CorporationID = corporationID;
        this.TitleID       = titleID;
        this.Changes       = new PyDictionary<PyString, PyTuple>();
    }

    public OnTitleChanged AddChange(PyString column, PyDataType oldValue, PyDataType newValue)
    {
        this.Changes[column] = new PyTuple(2)
        {
            [0] = oldValue,
            [1] = newValue
        };

        return this;
    }

    public override List<PyDataType> GetElements()
    {
        return new List<PyDataType>()
        {
            this.CorporationID,
            this.TitleID,
            this.Changes
        };
    }
}