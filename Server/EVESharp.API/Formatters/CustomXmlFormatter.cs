using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace EVESharp.API.Formatters;

public class CustomXmlFormatter : XmlSerializerOutputFormatter
{
    public CustomXmlFormatter (XmlWriterSettings writerSettings) : base (writerSettings) { }

    protected override void Serialize (XmlSerializer xmlSerializer, XmlWriter xmlWriter, object? value)
    {
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();

        ns.Add ("", "");
        
        xmlSerializer.Serialize(xmlWriter, value, ns);
    }
}