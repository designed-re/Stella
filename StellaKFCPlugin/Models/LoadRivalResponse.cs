using System.Collections.Generic;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class LoadRivalResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        // asphyxia serialises `rival: [...]` as repeated <rival> elements.
        [XmlElement(ElementName = "rival")]
        public List<RivalEntry> Rivals { get; set; } = new();
    }

    public class RivalEntry
    {
        [XmlElement(ElementName = "no")]
        public short No { get; set; }

        [XmlElement(ElementName = "seq")]
        public string Seq { get; set; } = string.Empty;

        [XmlElement(ElementName = "name")]
        public string Name { get; set; } = string.Empty;

        // music is an array of {param} -> repeated <music> elements.
        [XmlElement(ElementName = "music")]
        public List<RivalMusic> Music { get; set; } = new();
    }

    public class RivalMusic
    {
        // param is a u32 array; XDocumentTypeExtensions adds __type="u32" __count.
        [XmlElement(ElementName = "param")]
        public List<uint> Param { get; set; } = new();
    }
}
