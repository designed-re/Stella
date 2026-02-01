using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "services")]
    public class GetServicesResponse : IStellaEAmuseResponse
    {
        [XmlElement(ElementName = "item")]
        public List<ServiceItem> Items { get; set; } = new();

        [XmlAttribute(AttributeName = "expire")]
        public int Expire { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "mode")]
        public string Mode { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public int Status { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class ServiceItem
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
    }
}

