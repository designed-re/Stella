using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "eacoin")]
    public class CheckInResponse : IStellaEAmuseResponse
    {
        public CheckInResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "balance")]
        public int Balance { get; set; }

        [XmlElement(ElementName = "sessid")]
        public string SessionId { get; set; }

        [XmlElement(ElementName = "acstatus")]
        public byte AcStatus { get; set; }

        [XmlElement(ElementName = "sequence")]
        public short Sequence { get; set; }

        [XmlElement(ElementName = "acid")]
        public string AcId { get; set; }

        [XmlElement(ElementName = "acname")]
        public string AcName { get; set; }
    }

    [XmlRoot(ElementName = "eacoin")]
    public class ConsumeResponse : IStellaEAmuseResponse
    {
        public ConsumeResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "balance")]
        public int Balance { get; set; }

        [XmlElement(ElementName = "autocharge")]
        public byte Autocharge { get; set; }

        [XmlElement(ElementName = "acstatus")]
        public byte AcStatus { get; set; }
    }

    // Request models
    [XmlRoot(ElementName = "eacoin")]
    public class CheckInRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "cardid")]
        public string CardId { get; set; }
    }

    [XmlRoot(ElementName = "eacoin")]
    public class ConsumeRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "sessid")]
        public string SessionId { get; set; }

        [XmlElement(ElementName = "payment")]
        public int Payment { get; set; }
    }
}
