using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "message")]
    public class GetMessageResponse : IStellaEAmuseResponse
    {
        [XmlElement(ElementName = "item")]
        public List<MessageItem> Items { get; set; } = new();

        [XmlAttribute(AttributeName = "expire")]
        public int Expire { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }

    [XmlRoot(ElementName = "item")]
    public class MessageItem
    {
        [XmlAttribute(AttributeName = "start")]
        public int Start { get; set; }

        [XmlAttribute(AttributeName = "end")]
        public int End { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}



