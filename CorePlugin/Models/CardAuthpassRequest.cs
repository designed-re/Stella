using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "cardmng")]
    public class CardAuthpassRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "pass")]
        public string Pass { get; set; }

        [XmlAttribute(AttributeName = "refid")]
        public string RefId { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
    }
}
