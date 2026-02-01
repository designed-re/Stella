using Stella.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StellaKFCPlugin.Models
{
    [XmlRoot(ElementName = "game")]
    public class LoadResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "result")]
        public byte Result { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "sdvx_id")]
        public string SdvxId { get; set; }

        [XmlElement(ElementName = "gamecoin_packet")]
        public uint GamecoinPacket { get; set; }

        [XmlElement(ElementName = "gamecoin_block")]
        public uint GamecoinBlock { get; set; }

        [XmlElement(ElementName = "appeal_id")]
        public ushort AppealId { get; set; }

        [XmlElement(ElementName = "last_music_id")]
        public int LastMusicId { get; set; }

        [XmlElement(ElementName = "last_music_type")]
        public byte LastMusicType { get; set; }

        [XmlElement(ElementName = "sort_type")]
        public byte SortType { get; set; }

        [XmlElement(ElementName = "headphone")]
        public byte Headphone { get; set; }

        [XmlElement(ElementName = "blaster_energy")]
        public uint BlasterEnergy { get; set; }

        [XmlElement(ElementName = "blaster_count")]
        public uint BlasterCount { get; set; }

        [XmlElement(ElementName = "extrack_energy")]
        public ushort ExtrackEnergy { get; set; }

        [XmlElement(ElementName = "hispeed")]
        public int Hispeed { get; set; }

        [XmlElement(ElementName = "lanespeed")]
        public uint Lanespeed { get; set; }

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

        [XmlElement(ElementName = "narrow_down")]
        public byte NarrowDown { get; set; }

        [XmlElement(ElementName = "kac_id")]
        public string KacId { get; set; }

        [XmlElement(ElementName = "skill_level")]
        public short SkillLevel { get; set; }

        [XmlElement(ElementName = "skill_base_id")]
        public short SkillBaseId { get; set; }

        [XmlElement(ElementName = "skill_name_id")]
        public short SkillNameId { get; set; }

        [XmlElement(ElementName = "ea_shop")]
        public EaShop EaShop { get; set; } = new();

        [XmlElement(ElementName = "eaappli")]
        public Eaappli Eaappli { get; set; } = new();

        [XmlElement(ElementName = "cloud")]
        public Cloud Cloud { get; set; } = new();

        [XmlElement(ElementName = "block_no")]
        public int BlockNo { get; set; }

        [XmlElement(ElementName = "skill")]
        public Skill Skill { get; set; } = new();

        [XmlElement(ElementName = "item")]
        public ItemElement Item { get; set; } = new();

        [XmlElement(ElementName = "param")]
        public ParamElement Param { get; set; } = new();

        [XmlElement(ElementName = "play_count")]
        public uint PlayCount { get; set; }

        [XmlElement(ElementName = "day_count")]
        public uint DayCount { get; set; }

        [XmlElement(ElementName = "today_count")]
        public uint TodayCount { get; set; }

        [XmlElement(ElementName = "play_chain")]
        public uint PlayChain { get; set; }

        [XmlElement(ElementName = "max_play_chain")]
        public uint MaxPlayChain { get; set; }

        [XmlElement(ElementName = "week_count")]
        public uint WeekCount { get; set; }

        [XmlElement(ElementName = "week_play_count")]
        public uint WeekPlayCount { get; set; }

        [XmlElement(ElementName = "week_chain")]
        public uint WeekChain { get; set; }

        [XmlElement(ElementName = "max_week_chain")]
        public uint MaxWeekChain { get; set; }

        [XmlElement(ElementName = "valgene_ticket")]
        public ValgeneTicket ValgeneTicket { get; set; } = new();
    }

    [XmlRoot(ElementName = "ea_shop")]
    public class EaShop
    {
        [XmlElement(ElementName = "packet_booster")]
        public int PacketBooster { get; set; }

        [XmlElement(ElementName = "blaster_pass_enable")]
        public bool BlasterPassEnable { get; set; }

        [XmlElement(ElementName = "blaster_pass_limit_date")]
        public ulong BlasterPassLimitDate { get; set; }
    }

    [XmlRoot(ElementName = "eaappli")]
    public class Eaappli
    {
        [XmlElement(ElementName = "relation")]
        public sbyte Relation { get; set; }
    }

    [XmlRoot(ElementName = "cloud")]
    public class Cloud
    {
        [XmlElement(ElementName = "relation")]
        public sbyte Relation { get; set; }
    }

    [XmlRoot(ElementName = "skill")]
    public class Skill
    {
        [XmlElement(ElementName = "course")]
        public List<SkillCourse> Course { get; set; } = new();
    }

    [XmlRoot(ElementName = "course")]
    public class SkillCourse
    {
        [XmlElement(ElementName = "ssnid")]
        public short Ssnid { get; set; }

        [XmlElement(ElementName = "crsid")]
        public short Crsid { get; set; }

        [XmlElement(ElementName = "st")]
        public short St { get; set; }

        [XmlElement(ElementName = "sc")]
        public int Sc { get; set; }

        [XmlElement(ElementName = "ex")]
        public int Ex { get; set; }

        [XmlElement(ElementName = "ct")]
        public short Ct { get; set; }

        [XmlElement(ElementName = "gr")]
        public short Gr { get; set; }

        [XmlElement(ElementName = "ar")]
        public short Ar { get; set; }

        [XmlElement(ElementName = "cnt")]
        public short Cnt { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class ItemElement
    {
        [XmlElement(ElementName = "info")]
        public List<ItemInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class ItemInfo
    {
        [XmlElement(ElementName = "type")]
        public byte Type { get; set; }

        [XmlElement(ElementName = "id")]
        public uint Id { get; set; }

        [XmlElement(ElementName = "param")]
        public uint Param { get; set; }
    }

    [XmlRoot(ElementName = "param")]
    public class ParamElement
    {
        [XmlElement(ElementName = "info")]
        public List<ParamInfo> Infos { get; set; } = new();
    }

    [XmlRoot(ElementName = "info")]
    public class ParamInfo
    {
        [XmlElement(ElementName = "type")]
        public int Type { get; set; }

        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        [XmlElement(ElementName = "param")]
        public List<int> Param { get; set; } = new();
    }

    [XmlRoot(ElementName = "valgene_ticket")]
    public class ValgeneTicket
    {
        [XmlElement(ElementName = "ticket_num")]
        public int TicketNum { get; set; }
        [XmlElement(ElementName = "limit_date")]
        public ulong LimitDate { get; set; }
    }
}

