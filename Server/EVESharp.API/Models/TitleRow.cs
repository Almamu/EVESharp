using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class TitleRow
{
    [XmlAttribute("titleID")]
    public long ID { get; set; }
    [XmlAttribute("titleName")]
    public string Name { get; set; }
}