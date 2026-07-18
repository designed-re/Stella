using System;
using System.Collections.Generic;
using StellaKFCPlugin.EF;

namespace StellaKFCPlugin.WebUI.ViewModels;

public sealed class DataModel
{
    public int StaticEventCount { get; set; }
    public int MusicCount { get; set; }
    public bool MusicDbPresent { get; set; }
}

public sealed class SongsListModel
{
    public int Version { get; set; }
    public IReadOnlyList<SongRow> Songs { get; set; } = Array.Empty<SongRow>();
    public sealed record SongRow(int Id, string Title, string Artist, int Version, string Date);
}

public sealed class StartupFlagsModel
{
    public StellaKFCPluginConfig Config { get; set; } = new();
}

public sealed class UnlockEventsModel
{
    public IReadOnlyList<EventRow> Events { get; set; } = Array.Empty<EventRow>();
    public sealed record EventRow(int Version, string EventId, string Type, int MinVersion, int StartDate, bool Enabled, string? Name, bool IsPrefix, IReadOnlyList<SubItem> SubItems);
    public sealed record SubItem(string Key, bool Toggled);
}

public sealed class WeeklyScoreAttackModel
{
    public IReadOnlyList<WeeklyRow> Rows { get; set; } = Array.Empty<WeeklyRow>();
    public sealed record WeeklyRow(int Version, int Week, int Mid, int Mtype, int Exscore, string Name, int PlayCount);
}

public sealed class ProfileDetailModel
{
    public string RefId { get; set; } = "";
    public SvProfile? Profile { get; set; }
    public int ScoreCount { get; set; }
    public int ItemCount { get; set; }
    public int RivalCount { get; set; }
}

public sealed class ProfileScoreModel
{
    public string RefId { get; set; } = "";
    public IReadOnlyList<ScoreRow> Scores { get; set; } = Array.Empty<ScoreRow>();
    public sealed record ScoreRow(int Mid, int Type, int Score, int Exscore, int Clear, int Grade, int Volforce, int PlayCount);
}

public sealed class ProfileSkillModel
{
    public string RefId { get; set; } = "";
    public int SkillLevel { get; set; }
    public int SkillBaseId { get; set; }
    public IReadOnlyList<SkillRow> Skills { get; set; } = Array.Empty<SkillRow>();
    public sealed record SkillRow(int Type, short Base, short Level, short Name);
}

public sealed class ProfileAchievementsModel
{
    public string RefId { get; set; } = "";
    public IReadOnlyList<ItemRow> Items { get; set; } = Array.Empty<ItemRow>();
    public sealed record ItemRow(byte Type, uint ItemId, uint Param);
}

public sealed class ProfileRivalsModel
{
    public string RefId { get; set; } = "";
    public IReadOnlyList<RivalRow> Rivals { get; set; } = Array.Empty<RivalRow>();
    public sealed record RivalRow(string RivalRefId, string Name, int SdvxId, bool Mutual, int Version);
}

public sealed class ProfileCustomizationModel
{
    public string RefId { get; set; } = "";
    public string? Name { get; set; }
    public int Version { get; set; }
    public int AppealId { get; set; }
    public IReadOnlyList<ParamRow> Params { get; set; } = Array.Empty<ParamRow>();
    public sealed record ParamRow(int Type, int ParamId, string Param, uint ParamCount);
}

public sealed class ProfileValkyrieGeneratorModel
{
    public string RefId { get; set; } = "";
    public IReadOnlyList<ValkRow> ValkSongs { get; set; } = Array.Empty<ValkRow>();
    public sealed record ValkRow(int MusicId, int Version);
}

public sealed class ProfilePremiumGeneratorModel
{
    public string RefId { get; set; } = "";
    public IReadOnlyList<PremRow> PreGenes { get; set; } = Array.Empty<PremRow>();
    public sealed record PremRow(int Version, int ApigeneId, string Name);
}
