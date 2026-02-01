using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class FrozenResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }
}
