using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "cardmng")]
    public class CardBindModelRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "refid")]
        public string RefId { get; set; }
    }

    [XmlRoot(ElementName = "cardmng")]
    public class CardBindModelResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlAttribute(AttributeName = "dataid")]
        public string DataId { get; set; }
    }
}