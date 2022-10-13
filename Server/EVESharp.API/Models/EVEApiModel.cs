using System.Xml.Serialization;

namespace EVESharp.API.Models;

[XmlRoot("eveapi")]
public class EVEApiModel<T>
{
    [XmlAttribute("version")]
    public int Version { get; init; } = 2;
    [XmlElement("currentTime")]
    public DateTime CurrentTime { get; init; } = DateTime.Now;
    [XmlElement("result")]
    public T?  Result      { get; init; }
    [XmlElement("cachedUntil")]
    public DateTime CachedUntil { get; init; } = DateTime.Now;
}