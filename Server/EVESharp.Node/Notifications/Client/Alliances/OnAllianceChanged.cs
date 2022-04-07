using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Alliances;

public class OnAllianceChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnAllianceChanged";

    public int          AllianceID { get; init; }
    public PyDictionary Changes    { get; init; }

    public OnAllianceChanged (int allianceID) : base (NOTIFICATION_NAME)
    {
        AllianceID = allianceID;
        Changes    = new PyDictionary ();
    }

    public OnAllianceChanged AddChange (string changeName, PyDataType oldValue, PyDataType newValue)
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
            Changes
        };
    }
}