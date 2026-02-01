using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "package")]
    public class GetPackageListRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "pkgtype")]
        public string Pkgtype { get; set; }
    }
}
