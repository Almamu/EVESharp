using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class SkillQueueRow
{
    [XmlAttribute("queuePosition")]
    public int Position { get; init; }
    [XmlAttribute("typeID")]
    public int TypeID { get; init; }
    [XmlAttribute("level")]
    public int Level  { get; init; }
    [XmlAttribute("startSP")]
    public double StartSP { get; init; }
    [XmlAttribute("endSP")]
    public double EndSP { get; init; }
    [XmlAttribute("startTime")]
    public DateTime StartTime { get; init; }
    [XmlAttribute("endTime")]
    public DateTime EndTime { get; init; }
}