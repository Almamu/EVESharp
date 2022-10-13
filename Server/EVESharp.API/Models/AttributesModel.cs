using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class AttributesModel
{
    [XmlElement("intelligence")]
    public int Intelligence { get; set; }
    [XmlElement("memory")]
    public int Memory { get; set; }
    [XmlElement("charisma")]
    public int Charisma { get; set; }
    [XmlElement("perception")]
    public int Perception { get; set; }
    [XmlElement("willpower")]
    public int Willpower { get; set; }
}