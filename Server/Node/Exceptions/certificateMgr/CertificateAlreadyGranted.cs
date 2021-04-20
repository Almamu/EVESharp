using EVE.Packets.Exceptions;

namespace Node.Exceptions.certificateMgr
{
    public class CertificateAlreadyGranted : UserError
    {
        public CertificateAlreadyGranted() : base("CertificateAlreadyGranted")
        {
        }
    }
}