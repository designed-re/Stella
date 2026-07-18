using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stella.Abstractions;
using StellaKFCPlugin.EF.StaticData;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class GetCommonResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "valgene")]
        public ValgeneElement Valgene { get; set; } = new();

        [XmlElement(ElementName = "arena")]
        public ArenaElement Arena { get; set; } = new();

        [XmlElement(ElementName = "event")]
        public EventElement Event { get; set; } = new();

        [XmlElement(ElementName = "extend")]
        public ExtendElement Extend { get; set; } = new();

        [XmlElement(ElementName = "music")]
        public MusicOverrideElement Music { get; set; } = new();

        [XmlElement(ElementName = "music_limited")]
        public MusicLimitedElement MusicLimited { get; set; } = new();

        [XmlElement(ElementName = "skill_course")]
        public SkillCourseElement SkillCourse { get; set; } = new();

        [XmlElement(ElementName = "weekly_music")]
        public List<WeeklyMusicInfo> WeeklyMusic { get; set; } = new();

        [XmlElement(ElementName = "apigene")]
        public ApigeneElement? Apigene { get; set; }
    }

    [XmlRoot(ElementName = "apigene")]
    public class ApigeneElement
    {
        [XmlElement(ElementName = "info")]
        public List<ApigeneInfo> Infos { get; set; } = new();

        [XmlElement(ElementName = "catalog")]
        public List<ApigeneCatalog> Catalogs { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class ApigeneInfo
    {
        [XmlElement(ElementName = "apigene_id")]
        public int ApigeneId { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement(ElementName = "name_english")]
        public string NameEnglish { get; set; } = string.Empty;

        [XmlElement(ElementName = "common_rate")]
        public int CommonRate { get; set; }

        [XmlElement(ElementName = "uncommon_rate")]
        public int UncommonRate { get; set; }

        [XmlElement(ElementName = "rare_rate")]
        public int RareRate { get; set; }

        [XmlElement(ElementName = "price")]
        public int Price { get; set; }

        [XmlElement(ElementName = "no_duplicate")]
        public bool NoDuplicate { get; set; }
    }

    [XmlRoot(ElementName = "catalog")]
    public class ApigeneCatalog
    {
        [XmlElement(ElementName = "apigene_id")]
        public int ApigeneId { get; set; }

        [XmlElement(ElementName = "rarity")]
        public int Rarity { get; set; }

        [XmlElement(ElementName = "item_type")]
        public int ItemType { get; set; }

        [XmlElement(ElementName = "item_id")]
        public int ItemId { get; set; }
    }

    /// <summary>
    /// <c>music</c> block of the common response. asphyxia
    /// (<c>common.ts</c> L319-343) builds <c>music: { info: musicOverride }</c>
    /// where <c>musicOverride</c> is a flat array alternating two kinds of
    /// <c>&lt;info&gt;</c> elements per overridden song:
    /// <list type="number">
    /// <item>An <b>info</b> element holding the song's top-level fields (every
    ///   key except <c>charts</c>/<c>start</c>), each wrapped with
    ///   <c>createItem</c> (<c>str</c> for strings, <c>u16</c> for
    ///   <c>volume</c>, otherwise <c>u32</c>).</item>
    /// <item>A <b>chart</b> element whose children are the difficulty names
    ///   (<c>NOVICE/ADVANCED/EXHAUST/INFINITE/MAXIMUM/ULTIMATE</c>), each
    ///   holding that chart's fields (e.g. <c>price</c>) wrapped with
    ///   <c>createItem</c>.</item>
    /// </list>
    /// This heterogeneous array cannot be expressed with typed C# models, so
    /// the element implements <see cref="IXmlSerializable"/> and writes the
    /// <c>__type</c> attributes itself (the framework's reflection-based type
    /// pass skips it because it exposes no public properties).
    /// </summary>
    [XmlRoot(ElementName = "music")]
    public class MusicOverrideElement : IXmlSerializable
    {
        // Held as a field (not a property) so the framework's reflection-based
        // __type pass does not recurse into the <info> children.
        public List<SvMusicOverride> Overrides = new();

        private static readonly Dictionary<string, string> DifficultyNames = new()
        {
            { "nov", "NOVICE" }, { "adv", "ADVANCED" }, { "exh", "EXHAUST" },
            { "inf", "INFINITE" }, { "mxm", "MAXIMUM" }, { "ult", "ULTIMATE" },
        };

        public XmlSchema? GetSchema() => null;

        public void ReadXml(XmlReader reader) => reader.Skip();

        public void WriteXml(XmlWriter writer)
        {
            foreach (var song in Overrides)
            {
                JObject info;
                try { info = JObject.Parse(string.IsNullOrEmpty(song.InfoJson) ? "{}" : song.InfoJson); }
                catch { info = new JObject(); }
                JObject charts;
                try { charts = JObject.Parse(string.IsNullOrEmpty(song.ChartsJson) ? "{}" : song.ChartsJson); }
                catch { charts = new JObject(); }

                // info element: song top-level fields.
                writer.WriteStartElement("info");
                foreach (var prop in info.Properties())
                    WriteKItem(writer, prop.Name, prop.Value);
                writer.WriteEndElement();

                // chart element: difficulty-name children holding chart fields.
                writer.WriteStartElement("info");
                foreach (var prop in charts.Properties())
                {
                    var name = DifficultyNames.TryGetValue(prop.Name, out var dn) ? dn : prop.Name.ToUpperInvariant();
                    writer.WriteStartElement(name);
                    if (prop.Value is JObject chartObj)
                    {
                        foreach (var cp in chartObj.Properties())
                            WriteKItem(writer, cp.Name, cp.Value);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        /// <summary>asphyxia <c>createItem</c>: str for strings, u16 for volume, u32 otherwise.</summary>
        private static void WriteKItem(XmlWriter writer, string key, JToken value)
        {
            writer.WriteStartElement(key);
            if (value.Type == JTokenType.String)
            {
                writer.WriteAttributeString("__type", "str");
                writer.WriteString(value.ToString());
            }
            else
            {
                writer.WriteAttributeString("__type", key == "volume" ? "u16" : "u32");
                // u32 must render unsigned; use the raw integer text.
                writer.WriteString(value.ToString());
            }
            writer.WriteEndElement();
        }
    }

    [XmlRoot(ElementName = "weekly_music")]
    public class WeeklyMusicInfo
    {
        [XmlElement(ElementName = "week_id")]
        public int WeekId { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "time_start")]
        public ulong TimeStart { get; set; }

        [XmlElement(ElementName = "time_end")]
        public ulong TimeEnd { get; set; }
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

        [XmlElement(ElementName = "rarity")]
        public int Rarity { get; set; }

        [XmlElement(ElementName = "item_type")]
        public int ItemType { get; set; }

        [XmlElement(ElementName = "item_id")]
        public int ItemId { get; set; }
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

