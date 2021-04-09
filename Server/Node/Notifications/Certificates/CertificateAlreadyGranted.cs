using PythonTypes.Types.Exceptions;

namespace Node.Notifications.Certificates
{
    public class CertificateAlreadyGranted : UserError
    {
        public CertificateAlreadyGranted() : base("CertificateAlreadyGranted")
        {
        }
    }
}