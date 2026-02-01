using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class LoadRequest : IStellaEAmuseRequest
    {

        [XmlElement(ElementName = "dataid")]
        public string Dataid { get; set; }

        [XmlElement(ElementName = "cardid")]
        public string Cardid { get; set; }

        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }

        [XmlElement(ElementName = "cardno")]
        public string Cardno { get; set; }

        [XmlElement(ElementName = "locid")]
        public string Locid { get; set; }

        [XmlAttribute(AttributeName = "k")]
        public string K { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "ver")]
        public int Ver { get; set; }
    }
}
