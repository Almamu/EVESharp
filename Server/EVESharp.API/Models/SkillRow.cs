using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class SkillRow
{
    [XmlAttribute("typeID")]
    public int TypeID { get; set; }
    [XmlAttribute("skillpoints")]
    public double SkillPoints { get; set; }
    [XmlAttribute("level")]
    public int Level { get; set; }
    [XmlAttribute("published")]
    public bool Published { get; set; }
}