using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Certificates.Notifications
{
    public class CertificateAlreadyGranted : UserError
    {
        public CertificateAlreadyGranted() : base("CertificateAlreadyGranted")
        {
        }
    }
}