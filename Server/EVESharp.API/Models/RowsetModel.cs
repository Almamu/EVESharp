using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class RowsetModel <T>
{
    [XmlElement("rowset")]
    public Rowset<T> Rowset { get; init; } = new Rowset <T> ();
}