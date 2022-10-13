using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class CertificateRow
{
    [XmlAttribute("certificateID")]
    public int CertificateID { get; set; }
}