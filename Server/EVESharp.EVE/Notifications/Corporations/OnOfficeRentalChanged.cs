using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnOfficeRentalChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnOfficeRentalChanged";

    public int  CorporationID { get; init; }
    public int? OfficeID      { get; init; }
    public int? FolderID      { get; init; }

    public OnOfficeRentalChanged (int corporationID, int? officeID, int? folderID) : base (NOTIFICATION_NAME)
    {
        this.CorporationID = corporationID;
        this.OfficeID      = officeID;
        this.FolderID      = folderID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.CorporationID,
            this.OfficeID,
            this.FolderID
        };
    }
}