using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SaveMRequest : IStellaEAmuseRequest
    {
        public SaveMRequest() { }

        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; }

        [XmlElement(ElementName = "dataid")]
        public string DataId { get; set; }

        [XmlElement(ElementName = "locid")]
        public string LocId { get; set; }

        [XmlElement(ElementName = "track")]
        public Track Track { get; set; } = new();
    }

    [XmlRoot(ElementName = "track")]
    public class Track
    {
        public Track() { }

        [XmlElement(ElementName = "play_id")]
        public uint PlayId { get; set; }

        [XmlElement(ElementName = "track_no")]
        public ushort TrackNo { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public int MusicType { get; set; }

        [XmlElement(ElementName = "score")]
        public int Score { get; set; }

        [XmlElement(ElementName = "exscore")]
        public int ExScore { get; set; }

        [XmlElement(ElementName = "clear_type")]
        public int ClearType { get; set; }

        [XmlElement(ElementName = "score_grade")]
        public int ScoreGrade { get; set; }

        [XmlElement(ElementName = "max_chain")]
        public uint MaxChain { get; set; }

        [XmlElement(ElementName = "just")]
        public uint Just { get; set; }

        [XmlElement(ElementName = "critical")]
        public uint Critical { get; set; }

        [XmlElement(ElementName = "near")]
        public uint Near { get; set; }

        [XmlElement(ElementName = "error")]
        public uint Error { get; set; }

        [XmlElement(ElementName = "effective_rate")]
        public uint EffectiveRate { get; set; }

        [XmlElement(ElementName = "btn_rate")]
        public int BtnRate { get; set; }

        [XmlElement(ElementName = "long_rate")]
        public int LongRate { get; set; }

        [XmlElement(ElementName = "vol_rate")]
        public int VolRate { get; set; }

        [XmlElement(ElementName = "mode")]
        public byte Mode { get; set; }

        [XmlElement(ElementName = "start_option")]
        public byte StartOption { get; set; }

        [XmlElement(ElementName = "gauge_type")]
        public byte GaugeType { get; set; }

        [XmlElement(ElementName = "notes_option")]
        public byte NotesOption { get; set; }

        [XmlElement(ElementName = "online_num")]
        public ushort OnlineNum { get; set; }

        [XmlElement(ElementName = "local_num")]
        public ushort LocalNum { get; set; }

        [XmlElement(ElementName = "challenge_type")]
        public byte ChallengeType { get; set; }

        [XmlElement(ElementName = "retry_cnt")]
        public int RetryCnt { get; set; }

        [XmlElement(ElementName = "judge")]
        public int[] Judge { get; set; }

        [XmlElement(ElementName = "drop_frame")]
        public ushort DropFrame { get; set; }

        [XmlElement(ElementName = "drop_frame_max")]
        public ushort DropFrameMax { get; set; }

        [XmlElement(ElementName = "drop_count")]
        public ushort DropCount { get; set; }

        [XmlElement(ElementName = "etc")]
        public string Etc { get; set; }

        [XmlElement(ElementName = "mix_id")]
        public int MixId { get; set; }

        [XmlElement(ElementName = "mix_like")]
        public bool MixLike { get; set; }

        [XmlElement(ElementName = "matching")]
        public List<Matching> Matchings { get; set; } = new();
    }

    [XmlRoot(ElementName = "matching")]
    public class Matching
    {
        public Matching() { }

        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "score")]
        public int Score { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class SaveMResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }
}

