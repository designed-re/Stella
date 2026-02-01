using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "package")]
    public class GetPackageListResponse : IStellaEAmuseResponse
    {
        [XmlElement(ElementName = "item")]
        public List<PackageItem> Items { get; set; } = new();

        [XmlAttribute(AttributeName = "expire")]
        public int Expire { get; set; }

        // TODO: Add response properties here
    }

    [XmlRoot(ElementName = "item")]
    public class PackageItem
    {
        // TODO: Add package item properties here
    }
}
