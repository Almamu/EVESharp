using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.certificateMgr;

public class CertificateCertPrerequisitesNotMet : UserError
{
    public CertificateCertPrerequisitesNotMet() : base("CertificateCertPrerequisitesNotMet")
    {
    }
}