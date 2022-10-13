using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class CharacterRow
{
    [XmlAttribute("name")]
    public string Name { get; init; }
    [XmlAttribute("characterID")]
    public int CharacterID { get; init; }
    [XmlAttribute("corporationName")]
    public string CorporationName { get; init; }
    [XmlAttribute("corporationID")]
    public int CorporationID { get; init; }
}