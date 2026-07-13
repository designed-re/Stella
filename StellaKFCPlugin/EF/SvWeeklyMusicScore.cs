using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Weekly Music Score) for Sound Voltex - weekly music ranking.
/// </summary>
public partial class SvWeeklyMusicScore
{
    public int Id { get; set; }

    public string RefId { get; set; } = null!;

    public int Week { get; set; }

    public int Mid { get; set; }

    public int Mtype { get; set; }

    public int Version { get; set; }

    public int Exscore { get; set; }

    public string Name { get; set; } = null!;

    public int PlayCount { get; set; }

    public int HiscoreCount { get; set; }
}