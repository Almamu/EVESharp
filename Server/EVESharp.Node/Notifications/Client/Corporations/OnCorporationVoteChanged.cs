using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Corporations;

public class OnCorporationVoteChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationVoteChanged";

    public  int                              CorporationID { get; init; }
    public  int                              VoteCaseID    { get; init; }
    public  int                              CharacterID   { get; init; }
    private PyDictionary <PyString, PyTuple> Changes       { get; }

    public OnCorporationVoteChanged (int corporationID, int voteCaseID, int characterID) : base (NOTIFICATION_NAME)
    {
        CorporationID = corporationID;
        VoteCaseID    = voteCaseID;
        CharacterID   = characterID;
        Changes       = new PyDictionary <PyString, PyTuple> ();
    }

    public OnCorporationVoteChanged AddValue (string columnName, PyDataType oldValue, PyDataType newValue)
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
            VoteCaseID,
            CharacterID,
            Changes
        };
    }
}