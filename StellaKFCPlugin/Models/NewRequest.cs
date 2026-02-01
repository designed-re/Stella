using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class NewRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

    }
}
