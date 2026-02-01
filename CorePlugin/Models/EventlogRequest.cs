using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "eventlog")]
    public class EventlogRequest : IStellaEAmuseRequest
    {

        [XmlElement(ElementName = "retrycnt")]
        public int Retrycnt { get; set; }

        [XmlElement(ElementName = "data")]
        public Data Data { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlText]
        public string Text { get; set; }

    }
    [XmlRoot(ElementName = "data")]
    public class Data
    {

        [XmlElement(ElementName = "eventid")]
        public string Eventid { get; set; }

        [XmlElement(ElementName = "eventorder")]
        public int Eventorder { get; set; }

        [XmlElement(ElementName = "pcbtime")]
        public ulong Pcbtime { get; set; }

        [XmlElement(ElementName = "gamesession")]
        public long Gamesession { get; set; }

        [XmlElement(ElementName = "strdata1")]
        public string Strdata1 { get; set; }

        [XmlElement(ElementName = "strdata2")]
        public string Strdata2 { get; set; }

        [XmlElement(ElementName = "numdata1")]
        public long Numdata1 { get; set; }

        [XmlElement(ElementName = "numdata2")]
        public long Numdata2 { get; set; }
    }
}
