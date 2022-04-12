using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Certificates;

public class OnCertificateIssued : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCertificateIssued";

    public PyInteger CertificateID { get; }

    public OnCertificateIssued (PyInteger certificateID = null) : base (NOTIFICATION_NAME)
    {
        CertificateID = certificateID;
    }

    public override List <PyDataType> GetElements ()
    {
        if (CertificateID == null)
            return new List <PyDataType> ();

        return new List <PyDataType> {CertificateID};
    }
}