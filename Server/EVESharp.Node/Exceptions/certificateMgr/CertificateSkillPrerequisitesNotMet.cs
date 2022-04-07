using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.certificateMgr;

public class CertificateSkillPrerequisitesNotMet : UserError
{
    public CertificateSkillPrerequisitesNotMet() : base("CertificateSkillPrerequisitesNotMet")
    {
    }
}