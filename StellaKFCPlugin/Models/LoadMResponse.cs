using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class LoadMResponse : IStellaEAmuseResponse
    {
        public LoadMResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "music")]
        public MusicElement Music { get; set; } = new();
    }

    [XmlRoot(ElementName = "music")]
    public class MusicElement
    {
        public MusicElement() { }

        [XmlElement(ElementName = "info")]
        public List<MusicInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class MusicInfo
    {
        public MusicInfo() { }

        [XmlElement(ElementName = "param")]
        public List<uint> Param { get; set; } = new();
    }
}


