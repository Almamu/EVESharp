using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Certificates
{
    public class OnCertificateIssued : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCertificateIssued";
        
        public PyInteger CertificateID { get; }
        
        public OnCertificateIssued(PyInteger certificateID = null) : base(NOTIFICATION_NAME)
        {
            this.CertificateID = certificateID;
        }

        public override List<PyDataType> GetElements()
        {
            if (this.CertificateID == null)
                return new List<PyDataType>();
            
            return new List<PyDataType>() {this.CertificateID};
        }
    }
}