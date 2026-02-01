using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "services")]
    public class GetServicesRequest : IStellaEAmuseRequest
    {

        [XmlElement(ElementName = "info")]
        public Info Info { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlText]
        public string Text { get; set; }

    }
    [XmlRoot(ElementName = "AVS2")]
    public class AVS2
    {

        [XmlAttribute(AttributeName = "__type")]
        public string Type { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "info")]
    public class Info
    {

        [XmlElement(ElementName = "AVS2")]
        public AVS2 AVS2 { get; set; }
    }
}

