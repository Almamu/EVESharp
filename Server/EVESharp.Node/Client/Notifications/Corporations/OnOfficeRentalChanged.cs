using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Corporations;

public class OnOfficeRentalChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnOfficeRentalChanged";

    public int  CorporationID { get; init; }
    public int? OfficeID      { get; init; }
    public int? FolderID      { get; init; }

    public OnOfficeRentalChanged (int corporationID, int? officeID, int? folderID) : base (NOTIFICATION_NAME)
    {
        CorporationID = corporationID;
        OfficeID      = officeID;
        FolderID      = folderID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            CorporationID,
            OfficeID,
            FolderID
        };
    }
}