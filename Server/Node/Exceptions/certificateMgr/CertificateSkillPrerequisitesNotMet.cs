using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.certificateMgr
{
    public class CertificateSkillPrerequisitesNotMet : UserError
    {
        public CertificateSkillPrerequisitesNotMet() : base("CertificateSkillPrerequisitesNotMet")
        {
        }
    }
}