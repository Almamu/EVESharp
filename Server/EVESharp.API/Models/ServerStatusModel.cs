using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class ServerStatusModel
{
    [XmlElement("serverOpen")]
    public bool ServerOpen    { get; init; }
    [XmlElement("onlinePlayers")]
    public long  OnlinePlayers { get; init; }
}