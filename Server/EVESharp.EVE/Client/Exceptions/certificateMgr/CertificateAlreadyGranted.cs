using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.certificateMgr;

public class CertificateAlreadyGranted : UserError
{
    public CertificateAlreadyGranted () : base ("CertificateAlreadyGranted") { }
}