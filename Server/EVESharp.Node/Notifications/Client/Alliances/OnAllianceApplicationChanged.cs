using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Alliances;

public class OnAllianceApplicationChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnAllianceApplicationChanged";
        
    public int          AllianceID { get; init; }
    public int          CorpID     { get; init; }
    public PyDictionary Changes    { get; init; }
        
    public OnAllianceApplicationChanged(int allianceID, int corpID) : base(NOTIFICATION_NAME)
    {
        this.AllianceID = allianceID;
        this.CorpID     = corpID;
        this.Changes    = new PyDictionary();
    }

    public OnAllianceApplicationChanged AddChange(string changeName, PyDataType oldValue, PyDataType newValue)
    {
        this.Changes[changeName] = new PyTuple(2)
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
            this.AllianceID,
            this.CorpID,
            this.Changes
        };
    }
}