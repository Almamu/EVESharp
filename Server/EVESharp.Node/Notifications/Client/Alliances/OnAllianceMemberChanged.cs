using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Alliances;

public class OnAllianceMemberChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnAllianceMemberChanged";

    public int          AllianceID { get; init; }
    public int          CorpID     { get; init; }
    public PyDictionary Changes    { get; init; }

    public OnAllianceMemberChanged (int allianceID, int corpID) : base (NOTIFICATION_NAME)
    {
        AllianceID = allianceID;
        CorpID     = corpID;
        Changes    = new PyDictionary ();
    }

    public OnAllianceMemberChanged AddChange (string changeName, PyDataType oldValue, PyDataType newValue)
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