using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class FrozenRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }

        [XmlElement(ElementName = "sec")]
        public int Sec { get; set; }

        [XmlAttribute(AttributeName = "k")]
        public string K { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "ver")]
        public int Ver { get; set; }


    }
}
