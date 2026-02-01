using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class LoadRivalRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string Refid { get; set; }
    }
}
