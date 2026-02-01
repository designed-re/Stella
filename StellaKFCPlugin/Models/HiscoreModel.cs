using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using StellaKFCPlugin.Classes;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class HiscoreRequest : IStellaEAmuseRequest
    {
        public HiscoreRequest() { }

        [XmlElement(ElementName = "locid")]
        public string LocId { get; set; }

        [XmlElement(ElementName = "limit")]
        public uint Limit { get; set; }

        [XmlElement(ElementName = "offset")]
        public uint Offset { get; set; }
    }

    [XmlRoot(ElementName = "hiscore")]
    public class HiscoreResponse : IStellaEAmuseResponse
    {
        public HiscoreResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "sc")]
        public HiscoreScoreElement ScoreElement { get; set; } = new();
    }

    public class HiscoreScoreElement
    {
        public HiscoreScoreElement() { }

        [XmlElement(ElementName = "d")]
        public List<HiscoreScoreData> ScoreDataList { get; set; } = new();
    }

    public class HiscoreScoreData
    {
        public HiscoreScoreData() { }

        [XmlElement(ElementName = "id")]
        public uint Id { get; set; }

        [XmlElement(ElementName = "ty")]
        public uint Type { get; set; }

        [XmlElement(ElementName = "a_sq")]
        public string AsqSequence { get; set; }

        [XmlElement(ElementName = "a_nm")]
        public string ANameId { get; set; }

        [XmlElement(ElementName = "a_sc")]
        public uint AScore { get; set; }

        [XmlElement(ElementName = "l_sq")]
        public string LsqSequence { get; set; }

        [XmlElement(ElementName = "l_nm")]
        public string LNameId { get; set; }

        [XmlElement(ElementName = "l_sc")]
        public uint LScore { get; set; }
    }
}





