using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace EVESharp.API.Models;

[XmlInclude(typeof(Rowset<SkillRow>))]
[XmlInclude(typeof(Rowset<CorporationRoleRow>))]
[XmlInclude(typeof(Rowset<TitleRow>))]
[XmlInclude(typeof(Rowset<CertificateRow>))]
public class CharacterModel
{
    [XmlElement("characterID")]
    public int CharacterID { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("DoB")]
    public DateTime DateOfBirth { get; set; }
    [XmlElement("race")]
    public string Race { get; set; }
    [XmlElement("bloodline")]
    public string Bloodline { get; set; }
    [XmlElement("ancestry")]
    public string Ancestry { get; set; }
    [XmlElement("gender")]
    public string Gender { get; set; }
    [XmlElement("corporationName")]
    public string CorporationName { get; set; }
    [XmlElement("corporationID")]
    public int CorporationID { get; set; }
    [XmlElement("allianceName")]
    public string AllianceName { get; set; }
    [XmlElement("allianceID")]
    public int AllianceID { get; set; }
    [XmlElement("factionName")]
    public string FactionName { get; set; }
    [XmlElement("factionID")]
    public int FactionID { get; set; }
    [XmlElement("cloneName")]
    public string CloneName { get; set; }
    [XmlElement("cloneSkillPoints")]
    public double CloneSkillPoints { get; set; }
    [XmlElement("balance")]
    public double Balance { get; set; }
    // TODO: ADD ATTRIBUTE ENHANCERS
    [XmlElement("attributes")]
    public AttributesModel Attributes { get; set; }
    [XmlIgnore]
    public Rowset<SkillRow> Skills { get; init; } = new Rowset <SkillRow> ();
    [XmlIgnore]
    public Rowset<CertificateRow> Certificates { get; init; } = new Rowset <CertificateRow> ();
    [XmlIgnore]
    public Rowset<CorporationRoleRow> CorporationRoles { get; init; } = new Rowset <CorporationRoleRow> ();
    [XmlIgnore]
    public Rowset<CorporationRoleRow> CorporationRolesAtHQ { get; init; } = new Rowset <CorporationRoleRow> ();
    [XmlIgnore]
    public Rowset<CorporationRoleRow> CorporationRolesAtBase { get; init; } = new Rowset <CorporationRoleRow> ();
    [XmlIgnore]
    public Rowset<CorporationRoleRow> CorporationRolesAtOther { get; init; } = new Rowset <CorporationRoleRow> ();
    [XmlIgnore]
    public Rowset<TitleRow> Titles { get; init; } = new Rowset <TitleRow> ();
    
    // this element is here just so the data can be properly serialized
    [XmlElement ("rowset")]
    public object [] Rowset
    {
        get => new object [] {Skills, Certificates, CorporationRoles, CorporationRolesAtHQ, CorporationRolesAtBase, CorporationRolesAtOther, Titles};
        set {} // this set is required, otherwise the serializer won't take this one into account
    }
}