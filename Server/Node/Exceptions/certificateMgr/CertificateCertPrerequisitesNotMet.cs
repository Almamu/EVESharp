using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.certificateMgr
{
    public class CertificateCertPrerequisitesNotMet : UserError
    {
        public CertificateCertPrerequisitesNotMet() : base("CertificateCertPrerequisitesNotMet")
        {
        }
    }
}