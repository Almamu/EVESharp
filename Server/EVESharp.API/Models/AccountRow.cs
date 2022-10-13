using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class AccountRow
{
    [XmlAttribute("accountID")]
    public int AccountID { get; init; }
    [XmlAttribute("accountKey")]
    public int AccountKey { get; init; }
    [XmlAttribute("balance")]
    public double Balance { get; init; }
}