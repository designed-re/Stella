using Stella.Abstractions;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class ShopRequest : IStellaEAmuseRequest
    {
    }

    [XmlRoot(ElementName = "game")]
    public class ShopResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "nxt_time")]
        public uint NxtTime { get; set; } = 1000 * 5 * 60;
    }
}