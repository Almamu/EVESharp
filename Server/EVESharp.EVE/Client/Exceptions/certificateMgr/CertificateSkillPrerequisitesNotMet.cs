using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.certificateMgr;

public class CertificateSkillPrerequisitesNotMet : UserError
{
    public CertificateSkillPrerequisitesNotMet () : base ("CertificateSkillPrerequisitesNotMet") { }
}