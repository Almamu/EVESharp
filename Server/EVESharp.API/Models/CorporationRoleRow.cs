using System.Xml.Serialization;

namespace EVESharp.API.Models;

public class CorporationRoleRow
{
    [XmlAttribute("roleID")]
    public long RoleID { get; set; }
    [XmlAttribute("roleName")]
    public string RoleName { get; set; }
}