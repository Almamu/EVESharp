using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.certificateMgr;

public class CertificateCertPrerequisitesNotMet : UserError
{
    public CertificateCertPrerequisitesNotMet () : base ("CertificateCertPrerequisitesNotMet") { }
}