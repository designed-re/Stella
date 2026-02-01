using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class GetCommonRequest : IStellaEAmuseRequest
    {

        [XmlElement(ElementName = "locid")]
        public string Locid { get; set; }

        [XmlElement(ElementName = "cstcode")]
        public string Cstcode { get; set; }

        [XmlElement(ElementName = "cpycode")]
        public string Cpycode { get; set; }

        [XmlElement(ElementName = "hadid")]
        public string Hadid { get; set; }

        [XmlElement(ElementName = "licid")]
        public string Licid { get; set; }

        [XmlElement(ElementName = "actid")]
        public string Actid { get; set; }

        [XmlElement(ElementName = "regioncode")]
        public string Regioncode { get; set; }

        [XmlAttribute(AttributeName = "k")]
        public string K { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "ver")]
        public int Ver { get; set; }
    }
}
