using System.Collections.Generic;
using System.Xml.Serialization;
using Stella.Abstractions;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class GetCommonResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "valgene")]
        public ValgeneElement Valgene { get; set; } = new();

        [XmlElement(ElementName = "skill_course")]
        public SkillCourseElement SkillCourse { get; set; } = new();

        [XmlElement(ElementName = "event")]
        public EventElement Event { get; set; } = new();

        [XmlElement(ElementName = "arena")]
        public ArenaElement Arena { get; set; } = new();

        [XmlElement(ElementName = "extend")]
        public ExtendElement Extend { get; set; } = new();

        [XmlElement(ElementName = "music_limited")]
        public MusicLimitedElement MusicLimited { get; set; } = new();
    }

    [XmlRoot(ElementName = "valgene")]
    public class ValgeneElement
    {
        [XmlElement(ElementName = "info")]
        public List<ValgeneInfo> Infos { get; set; } = new();

        [XmlElement(ElementName = "catalog")]
        public List<ValgenesCatalog> Catalogs { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class ValgeneInfo
    {
        [XmlElement(ElementName = "valgene_name")]
        public string ValgeneName { get; set; }

        [XmlElement(ElementName = "valgene_name_english")]
        public string ValgeneNameEnglish { get; set; }

        [XmlElement(ElementName = "valgene_id")]
        public int ValgeneId { get; set; }
    }

    [XmlRoot(ElementName = "catalog")]
    public class ValgenesCatalog
    {
        [XmlElement(ElementName = "valgene_id")]
        public int ValgeneId { get; set; }

        [XmlElement(ElementName = "item_type")]
        public int ItemType { get; set; }

        [XmlElement(ElementName = "item_id")]
        public int ItemId { get; set; }

        [XmlElement(ElementName = "rarity")]
        public int Rarity { get; set; }
    }

    [XmlRoot(ElementName = "skill_course")]
    public class SkillCourseElement
    {
        [XmlElement(ElementName = "info")]
        public List<CourseInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class CourseInfo
    {
        [XmlElement(ElementName = "season_id")]
        public int SeasonId { get; set; }

        [XmlElement(ElementName = "season_name")]
        public string SeasonName { get; set; }

        [XmlElement(ElementName = "season_new_flg")]
        public bool SeasonNewFlg { get; set; }

        [XmlElement(ElementName = "course_type")]
        public short CourseType { get; set; }

        [XmlElement(ElementName = "course_id")]
        public short CourseId { get; set; }

        [XmlElement(ElementName = "course_name")]
        public string CourseName { get; set; }

        [XmlElement(ElementName = "skill_level")]
        public short SkillLevel { get; set; }

        [XmlElement(ElementName = "skill_type")]
        public short SkillType { get; set; }

        [XmlElement(ElementName = "skill_name_id")]
        public short SkillNameId { get; set; }

        [XmlElement(ElementName = "matching_assist")]
        public bool MatchingAssist { get; set; }

        [XmlElement(ElementName = "clear_rate")]
        public int ClearRate { get; set; }

        [XmlElement(ElementName = "avg_score")]
        public uint AvgScore { get; set; }

        [XmlElement(ElementName = "track")]
        public List<TrackInfo> Tracks { get; set; } = new();
    }

    [XmlRoot(ElementName = "track")]
    public class TrackInfo
    {
        [XmlElement(ElementName = "track_no")]
        public short TrackNo { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public sbyte MusicType { get; set; }
    }

    [XmlRoot(ElementName = "event")]
    public class EventElement
    {
        [XmlElement(ElementName = "info")]
        public List<EventInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class EventInfo
    {
        [XmlElement(ElementName = "event_id")]
        public string EventId { get; set; }
    }

    [XmlRoot(ElementName = "arena")]
    public class ArenaElement
    {
        [XmlElement(ElementName = "season")]
        public int Season { get; set; }

        [XmlElement(ElementName = "rule")]
        public int Rule { get; set; }

        [XmlElement(ElementName = "rank_match_target")]
        public int RankMatchTarget { get; set; }

        [XmlElement(ElementName = "time_start")]
        public ulong TimeStart { get; set; }

        [XmlElement(ElementName = "time_end")]
        public ulong TimeEnd { get; set; }

        [XmlElement(ElementName = "shop_start")]
        public ulong ShopStart { get; set; }

        [XmlElement(ElementName = "shop_end")]
        public ulong ShopEnd { get; set; }

        [XmlElement(ElementName = "is_open")]
        public bool IsOpen { get; set; }

        [XmlElement(ElementName = "is_shop")]
        public bool IsShop { get; set; }

        [XmlElement(ElementName = "catalog")]
        public List<ArenaCatalog> Catalogs { get; set; } = new();
    }

    [XmlRoot(ElementName = "catalog")]
    public class ArenaCatalog
    {
        [XmlElement(ElementName = "catalog_id")]
        public int CatalogId { get; set; }

        [XmlElement(ElementName = "catalog_type")]
        public int CatalogType { get; set; }

        [XmlElement(ElementName = "price")]
        public int Price { get; set; }

        [XmlElement(ElementName = "item_type")]
        public int ItemType { get; set; }

        [XmlElement(ElementName = "item_id")]
        public int ItemId { get; set; }

        [XmlElement(ElementName = "param")]
        public int Param { get; set; }
    }

    [XmlRoot(ElementName = "extend")]
    public class ExtendElement
    {
        [XmlElement(ElementName = "info")]
        public List<ExtendInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class ExtendInfo
    {
        [XmlElement(ElementName = "extend_id")]
        public uint ExtendId { get; set; }

        [XmlElement(ElementName = "extend_type")]
        public uint ExtendType { get; set; }

        [XmlElement(ElementName = "param_num_1")]
        public int ParamNum1 { get; set; }

        [XmlElement(ElementName = "param_num_2")]
        public int ParamNum2 { get; set; }

        [XmlElement(ElementName = "param_num_3")]
        public int ParamNum3 { get; set; }

        [XmlElement(ElementName = "param_num_4")]
        public int ParamNum4 { get; set; }

        [XmlElement(ElementName = "param_num_5")]
        public int ParamNum5 { get; set; }

        [XmlElement(ElementName = "param_str_1")]
        public string ParamStr1 { get; set; }

        [XmlElement(ElementName = "param_str_2")]
        public string ParamStr2 { get; set; }

        [XmlElement(ElementName = "param_str_3")]
        public string ParamStr3 { get; set; }

        [XmlElement(ElementName = "param_str_4")]
        public string ParamStr4 { get; set; }

        [XmlElement(ElementName = "param_str_5")]
        public string ParamStr5 { get; set; }
    }

    [XmlRoot(ElementName = "music_limited")]
    public class MusicLimitedElement
    {
        [XmlElement(ElementName = "info")]
        public List<MusicLimitedInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class MusicLimitedInfo
    {
        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public byte MusicType { get; set; }

        [XmlElement(ElementName = "limited")]
        public byte Limited { get; set; }
    }
}

