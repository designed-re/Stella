using System.Xml.Serialization;
using Stella.Abstractions;

namespace StellaKFCPlugin.Models;

/// <summary>
/// GRAVITY WARS (sv3) load response. Ported from asphyxia kfc/templates/load.pug
/// version === 3 block.
/// </summary>
[XmlRoot(ElementName = "game")]
public class Sv3LoadResponse : IStellaEAmuseResponse
{
    [XmlAttribute(AttributeName = "status")]
    public string Status { get; set; } = "0";

    [XmlElement(ElementName = "result")]
    public byte Result { get; set; }

    [XmlElement(ElementName = "name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement(ElementName = "code")]
    public string Code { get; set; } = string.Empty;

    [XmlElement(ElementName = "gamecoin_packet")]
    public uint GamecoinPacket { get; set; }

    [XmlElement(ElementName = "gamecoin_block")]
    public uint GamecoinBlock { get; set; }

    [XmlElement(ElementName = "skill_level")]
    public short SkillLevel { get; set; }

    [XmlElement(ElementName = "skill_name_id")]
    public short SkillNameId { get; set; }

    [XmlElement(ElementName = "hidden_param")]
    public HiddenParamElement? HiddenParam { get; set; }

    [XmlElement(ElementName = "play_count")]
    public uint PlayCount { get; set; }

    [XmlElement(ElementName = "daily_count")]
    public uint DayCount { get; set; }

    [XmlElement(ElementName = "play_chain")]
    public uint TodayCount { get; set; }

    [XmlElement(ElementName = "blaster_energy")]
    public uint BlasterEnergy { get; set; }

    [XmlElement(ElementName = "blaster_count")]
    public uint BlasterCount { get; set; }

    [XmlElement(ElementName = "last")]
    public Sv3LastElement Last { get; set; } = new();

    [XmlElement(ElementName = "creator_item")]
    public Sv3CreatorItemElement? CreatorItem { get; set; }

    [XmlElement(ElementName = "item")]
    public Sv3ItemElement Item { get; set; } = new();

    [XmlElement(ElementName = "skill")]
    public Sv3SkillElement Skill { get; set; } = new();

    [XmlElement(ElementName = "pb")]
    public Sv3PbElement Pb { get; set; } = new();

    [XmlElement(ElementName = "story")]
    public Sv3StoryElement? Story { get; set; }
}

[XmlRoot(ElementName = "hidden_param")]
public class HiddenParamElement
{
    [XmlAttribute(AttributeName = "__type")]
    public string Type { get; set; } = "s32";

    [XmlAttribute(AttributeName = "__count")]
    public int Count { get; set; }

    [XmlText]
    public string Value { get; set; } = "0";
}

[XmlRoot(ElementName = "last")]
public class Sv3LastElement
{
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

    [XmlElement(ElementName = "hispeed")]
    public int Hispeed { get; set; }

    [XmlElement(ElementName = "appeal_id")]
    public ushort AppealId { get; set; }

    [XmlElement(ElementName = "comment_id")]
    public ushort CommentId { get; set; }

    [XmlElement(ElementName = "gauge_option")]
    public byte GaugeOption { get; set; }
}

[XmlRoot(ElementName = "creator_item")]
public class Sv3CreatorItemElement
{
    [XmlElement(ElementName = "info")]
    public Sv3CreatorItemInfo Info { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3CreatorItemInfo
{
    [XmlElement(ElementName = "creator_type")]
    public uint CreatorType { get; set; }

    [XmlElement(ElementName = "item_id")]
    public uint ItemId { get; set; }

    [XmlElement(ElementName = "param")]
    public uint Param { get; set; }
}

[XmlRoot(ElementName = "item")]
public class Sv3ItemElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3ItemInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3ItemInfo
{
    [XmlElement(ElementName = "type")]
    public byte Type { get; set; }

    [XmlElement(ElementName = "id")]
    public uint Id { get; set; }

    [XmlElement(ElementName = "param")]
    public uint Param { get; set; }
}

[XmlRoot(ElementName = "skill")]
public class Sv3SkillElement
{
    [XmlElement(ElementName = "course_all")]
    public Sv3CourseAllElement? CourseAll { get; set; }
}

[XmlRoot(ElementName = "course_all")]
public class Sv3CourseAllElement
{
    [XmlElement(ElementName = "d")]
    public List<Sv3CourseElement> Courses { get; set; } = new();
}

[XmlRoot(ElementName = "d")]
public class Sv3CourseElement
{
    [XmlElement(ElementName = "ssnid")]
    public int Ssnid { get; set; }

    [XmlElement(ElementName = "crsid")]
    public short Crsid { get; set; }

    [XmlElement(ElementName = "ct")]
    public short ClearType { get; set; }

    [XmlElement(ElementName = "ar")]
    public short Rate { get; set; }
}

[XmlRoot(ElementName = "pb")]
public class Sv3PbElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3PbInfo> Infos { get; set; } = new();

    [XmlElement(ElementName = "energy")]
    public List<Sv3PbEnergyInfo> Energies { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3PbInfo
{
    [XmlElement(ElementName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement(ElementName = "title_eng")]
    public string TitleEng { get; set; } = string.Empty;

    [XmlElement(ElementName = "target_id")]
    public int TargetId { get; set; }

    [XmlElement(ElementName = "exp")]
    public int Exp { get; set; }

    [XmlElement(ElementName = "start_date")]
    public ulong StartDate { get; set; }

    [XmlElement(ElementName = "end_date")]
    public ulong EndDate { get; set; }

    [XmlElement(ElementName = "music")]
    public List<Sv3PbMusic> Music { get; set; } = new();
}

[XmlRoot(ElementName = "music")]
public class Sv3PbMusic
{
    [XmlElement(ElementName = "no")]
    public int No { get; set; }

    [XmlElement(ElementName = "point")]
    public int Point { get; set; }

    [XmlElement(ElementName = "music_id")]
    public int MusicId { get; set; }
}

[XmlRoot(ElementName = "energy")]
public class Sv3PbEnergyInfo
{
    [XmlElement(ElementName = "target_id")]
    public int TargetId { get; set; }

    [XmlElement(ElementName = "energy")]
    public int Energy { get; set; }
}

[XmlRoot(ElementName = "story")]
public class Sv3StoryElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3StoryInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3StoryInfo
{
    [XmlElement(ElementName = "story_id")]
    public int StoryId { get; set; }

    [XmlElement(ElementName = "progress_id")]
    public int ProgressId { get; set; }

    [XmlElement(ElementName = "progress_param")]
    public int ProgressParam { get; set; }

    [XmlElement(ElementName = "clear_cnt")]
    public int ClearCnt { get; set; }

    [XmlElement(ElementName = "route_flg")]
    public uint RouteFlg { get; set; }
}


/// <summary>
/// GRAVITY WARS (sv3) load_m response. Ported from asphyxia kfc/handlers/profiles.ts
/// loadScore version === 2 || version === 3 block.
/// v3 uses named fields in new/old sections instead of param arrays.
/// </summary>
[XmlRoot(ElementName = "game")]
public class Sv3LoadMResponse : IStellaEAmuseResponse
{
    [XmlAttribute(AttributeName = "status")]
    public string Status { get; set; } = "0";

    [XmlElement(ElementName = "new")]
    public Sv3LoadMNewElement New { get; set; } = new();

    [XmlElement(ElementName = "old")]
    public Sv3LoadMOldElement Old { get; set; } = new();
}

[XmlRoot(ElementName = "new")]
public class Sv3LoadMNewElement
{
    [XmlElement(ElementName = "music")]
    public List<Sv3LoadMMusicNew> Music { get; set; } = new();
}

[XmlRoot(ElementName = "music")]
public class Sv3LoadMMusicNew
{
    [XmlElement(ElementName = "music_id")]
    public uint MusicId { get; set; }

    [XmlElement(ElementName = "music_type")]
    public uint MusicType { get; set; }

    [XmlElement(ElementName = "score")]
    public uint Score { get; set; }

    [XmlElement(ElementName = "cnt")]
    public uint Cnt { get; set; }

    [XmlElement(ElementName = "clear_type")]
    public uint ClearType { get; set; }

    [XmlElement(ElementName = "score_grade")]
    public uint ScoreGrade { get; set; }

    [XmlElement(ElementName = "btn_rate")]
    public uint BtnRate { get; set; }

    [XmlElement(ElementName = "long_rate")]
    public uint LongRate { get; set; }

    [XmlElement(ElementName = "vol_rate")]
    public uint VolRate { get; set; }
}

[XmlRoot(ElementName = "old")]
public class Sv3LoadMOldElement
{
    [XmlElement(ElementName = "music")]
    public List<Sv3LoadMMusicOld> Music { get; set; } = new();
}

[XmlRoot(ElementName = "music")]
public class Sv3LoadMMusicOld
{
    [XmlElement(ElementName = "music_id")]
    public uint MusicId { get; set; }

    [XmlElement(ElementName = "music_type")]
    public uint MusicType { get; set; }

    [XmlElement(ElementName = "score")]
    public uint Score { get; set; }

    [XmlElement(ElementName = "cnt")]
    public uint Cnt { get; set; }

    [XmlElement(ElementName = "clear_type")]
    public uint ClearType { get; set; }

    [XmlElement(ElementName = "score_grade")]
    public uint ScoreGrade { get; set; }
}

/// <summary>
/// GRAVITY WARS (sv3) save request. Includes story progression data.
/// </summary>
[XmlRoot(ElementName = "game")]
public class Sv3SaveRequest : IStellaEAmuseRequest
{
    [XmlElement(ElementName = "refid")]
    public string RefId { get; set; } = string.Empty;

    [XmlElement(ElementName = "dataid")]
    public string DataId { get; set; } = string.Empty;

    [XmlElement(ElementName = "name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement(ElementName = "code")]
    public string Code { get; set; } = string.Empty;

    [XmlElement(ElementName = "gamecoin_packet")]
    public uint GamecoinPacket { get; set; }

    [XmlElement(ElementName = "gamecoin_block")]
    public uint GamecoinBlock { get; set; }

    [XmlElement(ElementName = "appeal_id")]
    public ushort AppealId { get; set; }

    [XmlElement(ElementName = "music_id")]
    public int MusicId { get; set; }

    [XmlElement(ElementName = "music_type")]
    public byte MusicType { get; set; }

    [XmlElement(ElementName = "sort_type")]
    public byte SortType { get; set; }

    [XmlElement(ElementName = "headphone")]
    public byte Headphone { get; set; }

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

    [XmlElement(ElementName = "skill_level")]
    public short SkillLevel { get; set; }

    [XmlElement(ElementName = "skill_name_id")]
    public short SkillNameId { get; set; }

    [XmlElement(ElementName = "earned_gamecoin_packet")]
    public uint EarnedGamecoinPacket { get; set; }

    [XmlElement(ElementName = "earned_gamecoin_block")]
    public uint EarnedGamecoinBlock { get; set; }

    [XmlElement(ElementName = "earned_blaster_energy")]
    public uint EarnedBlasterEnergy { get; set; }

    [XmlElement(ElementName = "item")]
    public Sv3SaveItemElement? Item { get; set; }

    [XmlElement(ElementName = "param")]
    public Sv3SaveParamElement? Param { get; set; }

    [XmlElement(ElementName = "pb")]
    public Sv3SavePbElement? Pb { get; set; }

    [XmlElement(ElementName = "story")]
    public Sv3SaveStoryElement? Story { get; set; }
}

[XmlRoot(ElementName = "item")]
public class Sv3SaveItemElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3SaveItemInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3SaveItemInfo
{
    [XmlElement(ElementName = "type")]
    public byte Type { get; set; }

    [XmlElement(ElementName = "id")]
    public uint Id { get; set; }

    [XmlElement(ElementName = "param")]
    public uint Param { get; set; }
}

[XmlRoot(ElementName = "param")]
public class Sv3SaveParamElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3SaveParamInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3SaveParamInfo
{
    [XmlElement(ElementName = "type")]
    public int Type { get; set; }

    [XmlElement(ElementName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "param")]
    // The game sends param as a KBinJSON __count array (space-joined s32).
    // PluginService.PreprocessXmlForArrays expands __count arrays into N
    // separate <param> elements before XmlSerializer runs, so a single string
    // field would only capture the first value. List<int> collects them all.
    public List<int> Param { get; set; } = new();
}

[XmlRoot(ElementName = "pb")]
public class Sv3SavePbElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3SavePbInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3SavePbInfo
{
    [XmlElement(ElementName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "exp")]
    public int Exp { get; set; }
}

[XmlRoot(ElementName = "story")]
public class Sv3SaveStoryElement
{
    [XmlElement(ElementName = "info")]
    public List<Sv3SaveStoryInfo> Infos { get; set; } = new();
}

[XmlRoot(ElementName = "info")]
public class Sv3SaveStoryInfo
{
    [XmlElement(ElementName = "story_id")]
    public int StoryId { get; set; }

    [XmlElement(ElementName = "progress_id")]
    public int ProgressId { get; set; }

    [XmlElement(ElementName = "progress_param")]
    public int ProgressParam { get; set; }

    [XmlElement(ElementName = "clear_cnt")]
    public int ClearCnt { get; set; }

    [XmlElement(ElementName = "route_flg")]
    public uint RouteFlg { get; set; }
}

/// <summary>
/// GRAVITY WARS (sv3) save_m request (music score save).
/// </summary>
[XmlRoot(ElementName = "game")]
public class Sv3SaveMRequest : IStellaEAmuseRequest
{
    [XmlElement(ElementName = "refid")]
    public string RefId { get; set; } = string.Empty;

    [XmlElement(ElementName = "dataid")]
    public string DataId { get; set; } = string.Empty;

    [XmlElement(ElementName = "music_id")]
    public int MusicId { get; set; }

    [XmlElement(ElementName = "music_type")]
    public int MusicType { get; set; }

    [XmlElement(ElementName = "score")]
    public int Score { get; set; }

    [XmlElement(ElementName = "clear_type")]
    public int ClearType { get; set; }

    [XmlElement(ElementName = "score_grade")]
    public int ScoreGrade { get; set; }

    [XmlElement(ElementName = "max_chain")]
    public uint MaxChain { get; set; }

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

    [XmlElement(ElementName = "gauge_type")]
    public byte GaugeType { get; set; }
}

/// <summary>
/// GRAVITY WARS (sv3) new profile request.
/// </summary>
[XmlRoot(ElementName = "game")]
public class Sv3NewRequest : IStellaEAmuseRequest
{
    [XmlElement(ElementName = "refid")]
    public string Refid { get; set; } = string.Empty;

    [XmlElement(ElementName = "dataid")]
    public string Dataid { get; set; } = string.Empty;

    [XmlElement(ElementName = "name")]
    public string Name { get; set; } = string.Empty;
}
