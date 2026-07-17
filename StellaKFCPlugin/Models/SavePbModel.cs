using Stella.Abstractions;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SavePbRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; } = string.Empty;

        [XmlElement(ElementName = "dataid")]
        public string DataId { get; set; } = string.Empty;

        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        [XmlElement(ElementName = "exp")]
        public int Exp { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class SavePbResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "exp")]
        public int Exp { get; set; }

        [XmlElement(ElementName = "result")]
        public bool Result { get; set; }
    }
}