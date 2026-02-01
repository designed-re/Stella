using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace CorePlugin.Models
{
    [XmlRoot(ElementName = "cardmng")]

    public class CardInquireRequest : IStellaEAmuseRequest
    {
        [XmlAttribute(AttributeName = "cardid")]
        public string Cardid { get; set; }

        [XmlAttribute(AttributeName = "cardtype")]
        public int Cardtype { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "update")]
        public int Update { get; set; }

    }
}
