using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "cardmng")]
    public class CardGetRefIdRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "cardid")]
        public string CardId { get; set; }

        [XmlAttribute(AttributeName = "passwd")]
        public string Passwd { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
    }
}
