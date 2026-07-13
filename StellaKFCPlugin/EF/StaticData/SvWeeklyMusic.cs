using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Weekly music definition (asphyxia weeklymusic.json).
/// </summary>
public partial class SvWeeklyMusic
{
    public int Id { get; set; }

    public int WeekId { get; set; }

    public int MusicId { get; set; }

    public long Start { get; set; }

    public long End { get; set; }
}