using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "facility")]
    public class GetFacilityResponse : IStellaEAmuseResponse
    {
        [XmlElement(ElementName = "location")]
        public Location Location { get; set; }

        [XmlElement(ElementName = "line")]
        public Line Line { get; set; }

        [XmlElement(ElementName = "portfw")]
        public PortFw PortFw { get; set; }

        [XmlElement(ElementName = "public")]
        public Public Public { get; set; }

        [XmlElement(ElementName = "share")]
        public Share Share { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }

        [XmlAttribute(AttributeName = "expire")]
        public int Expire { get; set; }
    }

    [XmlRoot(ElementName = "location")]
    public class Location
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "country")]
        public string Country { get; set; }

        [XmlElement(ElementName = "region")]
        public string Region { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "type")]
        public byte Type { get; set; }

        [XmlElement(ElementName = "countryname")]
        public string CountryName { get; set; }

        [XmlElement(ElementName = "countryjname")]
        public string CountryJName { get; set; }

        [XmlElement(ElementName = "regionname")]
        public string RegionName { get; set; }

        [XmlElement(ElementName = "regionjname")]
        public string RegionJName { get; set; }

        [XmlElement(ElementName = "customercode")]
        public string CustomerCode { get; set; }

        [XmlElement(ElementName = "companycode")]
        public string CompanyCode { get; set; }

        [XmlElement(ElementName = "latitude")]
        public int Latitude { get; set; }

        [XmlElement(ElementName = "longitude")]
        public int Longitude { get; set; }

        [XmlElement(ElementName = "accuracy")]
        public byte Accuracy { get; set; }
    }

    [XmlRoot(ElementName = "line")]
    public class Line
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "class")]
        public byte Class { get; set; }
    }

    [XmlRoot(ElementName = "portfw")]
    public class PortFw
    {
        [XmlElement(ElementName = "globalip")]
        public string GlobalIp { get; set; }

        [XmlElement(ElementName = "globalport")]
        public ushort GlobalPort { get; set; }

        [XmlElement(ElementName = "privateport")]
        public ushort PrivatePort { get; set; }
    }

    [XmlRoot(ElementName = "public")]
    public class Public
    {
        [XmlElement(ElementName = "flag")]
        public byte Flag { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "latitude")]
        public string Latitude { get; set; }

        [XmlElement(ElementName = "longitude")]
        public string Longitude { get; set; }
    }

    [XmlRoot(ElementName = "share")]
    public class Share
    {
        [XmlElement(ElementName = "eacoin")]
        public EaCoin EaCoin { get; set; }

        [XmlElement(ElementName = "url")]
        public Url Url { get; set; }
    }

    [XmlRoot(ElementName = "eacoin")]
    public class EaCoin
    {
        [XmlElement(ElementName = "notchamount")]
        public int NotchAmount { get; set; }

        [XmlElement(ElementName = "notchcount")]
        public int NotchCount { get; set; }

        [XmlElement(ElementName = "supplylimit")]
        public int SupplyLimit { get; set; }
    }

    [XmlRoot(ElementName = "url")]
    public class Url
    {
        [XmlElement(ElementName = "eapass")]
        public string EaPass { get; set; }

        [XmlElement(ElementName = "arcadefan")]
        public string ArcadeFan { get; set; }

        [XmlElement(ElementName = "konaminetdx")]
        public string KonaminetDx { get; set; }

        [XmlElement(ElementName = "konamiid")]
        public string KonamiId { get; set; }

        [XmlElement(ElementName = "eagate")]
        public string EaGate { get; set; }
    }
}


