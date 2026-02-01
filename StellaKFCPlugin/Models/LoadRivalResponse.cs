using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "entry")]
    public class LoadRivalResponse : IStellaEAmuseResponse
    {
        public LoadRivalResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }
}


