using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.certificateMgr
{
    public class CertificateAlreadyGranted : UserError
    {
        public CertificateAlreadyGranted() : base("CertificateAlreadyGranted")
        {
        }
    }
}