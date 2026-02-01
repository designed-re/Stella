using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "pcbtracker")]
    public class GetPcbTrackerResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "ecenable")]
        public bool ECEnable { get; set; } //this indicates paseli enabled or not

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


