using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Alliances;

public class OnAllianceApplicationChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnAllianceApplicationChanged";

    public int          AllianceID { get; init; }
    public int          CorpID     { get; init; }
    public PyDictionary Changes    { get; init; }

    public OnAllianceApplicationChanged (int allianceID, int corpID) : base (NOTIFICATION_NAME)
    {
        AllianceID = allianceID;
        CorpID     = corpID;
        Changes    = new PyDictionary ();
    }

    public OnAllianceApplicationChanged AddChange (string changeName, PyDataType oldValue, PyDataType newValue)
    {
        Changes [changeName] = new PyTuple (2)
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
            AllianceID,
            CorpID,
            Changes
        };
    }
}