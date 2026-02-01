using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "message")]
    public class GetMessageRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

    }
}


