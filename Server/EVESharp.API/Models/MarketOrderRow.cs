using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class MarketOrderRow
{
    [XmlAttribute("orderID")]
    public int OrderID { get; set; }
    [XmlAttribute("charID")]
    public int CharacterID { get; set; }
    [XmlAttribute("stationID")]
    public int StationID { get; set; }
    [XmlAttribute("volEntered")]
    public double VolEntered { get; set; }
    [XmlAttribute("volRemaining")]
    public double VolRemaining { get; set; }
    [XmlAttribute("minVolume")]
    public double MinVolume { get; set; }
    [XmlAttribute("orderState")]
    public int OrderState { get; set; }
    [XmlAttribute("typeID")]
    public int TypeID { get; set; }
    [XmlAttribute("range")]
    public int Range { get; set; }
    [XmlAttribute("accountKey")]
    public int AccountKey { get; set; }
    [XmlAttribute("duration")]
    public int Duration { get; set; }
    [XmlAttribute("escrow")]
    public double Escrow { get; set; }
    [XmlAttribute("price")]
    public double Price { get; set; }
    [XmlAttribute("bid")]
    public bool Bid { get; set; }
    [XmlAttribute("issued")]
    public DateTime Issued { get; set; }
}