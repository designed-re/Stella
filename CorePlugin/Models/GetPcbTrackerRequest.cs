using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "pcbtracker")]
    public class GetPcbTrackerRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "accountid")]
        public string Accountid { get; set; }
        [XmlAttribute(AttributeName = "ecflag")]
        public string Ecflag { get; set; }
        [XmlAttribute(AttributeName = "hardid")]
        public string Hardid { get; set; }
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
        [XmlAttribute(AttributeName = "softid")]
        public string Softid { get; set; }
    }
}


