using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class LoadMRequest : IStellaEAmuseRequest
    {
        public LoadMRequest() { }

        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }
    }
}

