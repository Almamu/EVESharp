using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class Rowset<T>
{
    [XmlAttribute("name")]
    public string Name { get; init; }
    [XmlAttribute("key")]
    public string Key { get; init; }
    [XmlAttribute("columns")]
    public string Columns { get; init; }
    [XmlElement("row")]
    public List<T> Rows { get; init; } = new List <T> ();
}