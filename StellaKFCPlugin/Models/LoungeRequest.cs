using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]

    public class LoungeRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "filter")]
        public byte Filter { get; set; } 
    }
}
