using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Corporations
{
    public class OnOfficeRentalChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnOfficeRentalChanged";
        
        public int CorporationID { get; init; }
        public int? OfficeID { get; init; }
        public int? FolderID { get; init; }
        
        public OnOfficeRentalChanged(int corporationID, int? officeID, int? folderID) : base(NOTIFICATION_NAME)
        {
            this.CorporationID = corporationID;
            this.OfficeID = officeID;
            this.FolderID = folderID;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.CorporationID,
                this.OfficeID,
                this.FolderID
            };
        }
    }
}