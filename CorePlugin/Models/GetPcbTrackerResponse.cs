using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "pcbtracker")]
    public class GetPcbTrackerResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlAttribute(AttributeName = "ecenable")]
        public int ECEnable { get; set; }

        [XmlAttribute(AttributeName = "eclimit")]
        public int ECLimit { get; set; }

        [XmlAttribute(AttributeName = "expire")]
        public int Expire { get; set; }

        [XmlAttribute(AttributeName = "limit")]
        public int Limit { get; set; }

        [XmlAttribute(AttributeName = "time")]
        public long Time { get; set; }
    }
}


