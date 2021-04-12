using PythonTypes.Types.Exceptions;

namespace Node.Notifications.Client.Certificates
{
    public class CertificateAlreadyGranted : UserError
    {
        public CertificateAlreadyGranted() : base("CertificateAlreadyGranted")
        {
        }
    }
}