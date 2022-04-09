using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.certificateMgr;

public class CertificateCertPrerequisitesNotMet : UserError
{
    public CertificateCertPrerequisitesNotMet () : base ("CertificateCertPrerequisitesNotMet") { }
}