using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "pcbevent")]
    public class GetPcbEventRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "time")]
        public Time Time { get; set; }

        [XmlElement(ElementName = "seq")]
        public Seq Seq { get; set; }

        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
    }

    [XmlRoot(ElementName = "time")]
    public class Time
    {

        [XmlText]
        public long Text { get; set; }
    }

    [XmlRoot(ElementName = "seq")]
    public class Seq
    {

        [XmlText]
        public long Text { get; set; }
    }

    [XmlRoot(ElementName = "name")]
    public class Name
    {

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "value")]
    public class Value
    {
        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {

        [XmlElement(ElementName = "name")]
        public Name Name { get; set; }

        [XmlElement(ElementName = "value")]
        public Value Value { get; set; }

        [XmlElement(ElementName = "time")]
        public Time Time { get; set; }
    }
}
