using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using StellaKFCPlugin.Classes;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class SaveRequest : IStellaEAmuseRequest
    {
        public SaveRequest() { }

        [XmlElement(ElementName = "play_id")]
        public uint PlayId { get; set; }

        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; }

        [XmlElement(ElementName = "locid")]
        public string LocId { get; set; }

        [XmlElement(ElementName = "appeal_id")]
        public ushort AppealId { get; set; }

        [XmlElement(ElementName = "skill_level")]
        public short SkillLevel { get; set; }

        [XmlElement(ElementName = "skill_base_id")]
        public short SkillBaseId { get; set; }

        [XmlElement(ElementName = "skill_name_id")]
        public short SkillNameId { get; set; }

        [XmlElement(ElementName = "skill_type")]
        public short SkillType { get; set; }

        [XmlElement(ElementName = "earned_gamecoin_packet")]
        public int EarnedGamecoinPacket { get; set; }

        [XmlElement(ElementName = "earned_gamecoin_block")]
        public int EarnedGamecoinBlock { get; set; }

        [XmlElement(ElementName = "earned_blaster_energy")]
        public int EarnedBlasterEnergy { get; set; }

        [XmlElement(ElementName = "variant_gate")]
        public SaveVariantGate VariantGate { get; set; } = new();

        [XmlElement(ElementName = "p_start")]
        public ulong PStart { get; set; }

        [XmlElement(ElementName = "p_end")]
        public ulong PEnd { get; set; }

        [XmlElement(ElementName = "ea_shop")]
        public SaveEaShop EaShop { get; set; } = new();

        [XmlElement(ElementName = "hispeed")]
        public int HiSpeed { get; set; }

        [XmlElement(ElementName = "lanespeed")]
        public uint LaneSpeed { get; set; }

        [XmlElement(ElementName = "gauge_option")]
        public byte GaugeOption { get; set; }

        [XmlElement(ElementName = "ars_option")]
        public byte ArsOption { get; set; }

        [XmlElement(ElementName = "notes_option")]
        public byte NotesOption { get; set; }

        [XmlElement(ElementName = "early_late_disp")]
        public byte EarlyLateDisp { get; set; }

        [XmlElement(ElementName = "draw_adjust")]
        public int DrawAdjust { get; set; }

        [XmlElement(ElementName = "eff_c_left")]
        public byte EffCLeft { get; set; }

        [XmlElement(ElementName = "eff_c_right")]
        public byte EffCRight { get; set; }

        [XmlElement(ElementName = "music_id")]
        public int MusicId { get; set; }

        [XmlElement(ElementName = "music_type")]
        public byte MusicType { get; set; }

        [XmlElement(ElementName = "sort_type")]
        public byte SortType { get; set; }

        [XmlElement(ElementName = "narrow_down")]
        public byte NarrowDown { get; set; }

        [XmlElement(ElementName = "headphone")]
        public byte Headphone { get; set; }

        [XmlElement(ElementName = "item")]
        public SaveItemElement Item { get; set; } = new();

        [XmlElement(ElementName = "param")]
        public SaveParamElement ParamElement { get; set; } = new();

        [XmlElement(ElementName = "print")]
        public SavePrintInfo Print { get; set; } = new();

        [XmlElement(ElementName = "start_option")]
        public sbyte StartOption { get; set; }

        [XmlElement(ElementName = "course")]
        public SaveCourseElement? Course { get; set; }
    }

    public class SaveCourseElement
    {
        [XmlElement(ElementName = "ssnid")]
        public short SeasonId { get; set; }

        [XmlElement(ElementName = "crsid")]
        public short CourseId { get; set; }

        [XmlElement(ElementName = "sc")]
        public int Score { get; set; }

        [XmlElement(ElementName = "ct")]
        public short Clear { get; set; }

        [XmlElement(ElementName = "gr")]
        public short Grade { get; set; }
        [XmlElement(ElementName = "ar")]
        public short Rate { get; set; }
    }

    public class SaveVariantGate
    {
        public SaveVariantGate() { }

        [XmlElement(ElementName = "earned_power")]
        public int EarnedPower { get; set; }

        [XmlElement(ElementName = "earned_element")]
        public SaveEarnedElement EarnedElement { get; set; } = new();
    }

    public class SaveEarnedElement
    {
        public SaveEarnedElement() { }

        [XmlElement(ElementName = "notes")]
        public int Notes { get; set; }

        [XmlElement(ElementName = "peak")]
        public int Peak { get; set; }

        [XmlElement(ElementName = "tsumami")]
        public int Tsumami { get; set; }

        [XmlElement(ElementName = "tricky")]
        public int Tricky { get; set; }

        [XmlElement(ElementName = "onehand")]
        public int OneHand { get; set; }

        [XmlElement(ElementName = "handtrip")]
        public int HandTrip { get; set; }
    }

    public class SaveEaShop
    {
        public SaveEaShop() { }

        [XmlElement(ElementName = "used_packet_booster")]
        public int UsedPacketBooster { get; set; }

        [XmlElement(ElementName = "used_block_booster")]
        public int UsedBlockBooster { get; set; }
    }

    public class SaveItemElement
    {
        public SaveItemElement() { }

        [XmlElement(ElementName = "info")]
        public List<SaveItemInfo> Infos { get; set; } = new();
    }

    public class SaveItemInfo
    {
        public SaveItemInfo() { }

        [XmlElement(ElementName = "id")]
        public uint Id { get; set; }

        [XmlElement(ElementName = "type")]
        public byte Type { get; set; }

        [XmlElement(ElementName = "param")]
        public uint Param { get; set; }
    }

    public class SaveParamElement
    {
        public SaveParamElement() { }

        [XmlElement(ElementName = "info")]
        public List<SaveParamInfo> Infos { get; set; } = new();
    }

    public class SaveParamInfo
    {
        public SaveParamInfo() { }

        [XmlElement(ElementName = "type")]
        public int Type { get; set; }

        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        [XmlElement(ElementName = "param")]
        public List<int> Params { get; set; } = new();
    }

    public class SavePrintInfo
    {
        public SavePrintInfo() { }

        [XmlElement(ElementName = "count")]
        public int Count { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class SaveResponse : IStellaEAmuseResponse
    {
        public SaveResponse() { }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }
}



