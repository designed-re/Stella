using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "cardmng")]

    public class CardInquireResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }

        [XmlAttribute(AttributeName = "binded")]
        public int Binded { get; set; }

        [XmlAttribute(AttributeName = "dataid")]
        public string Dataid { get; set; }

        [XmlAttribute(AttributeName = "ecflag")]
        public int Ecflag { get; set; }

        [XmlAttribute(AttributeName = "expired")]
        public int Expired { get; set; }

        [XmlAttribute(AttributeName = "newflag")]
        public int Newflag { get; set; }

        [XmlAttribute(AttributeName = "refid")]
        public string Refid { get; set; }

    }
}
