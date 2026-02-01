using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "facility")]
    public class GetFacilityRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "encoding")]
        public string Encoding { get; set; }
    }
}


