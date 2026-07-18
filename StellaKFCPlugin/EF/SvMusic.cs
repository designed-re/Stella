using System;
using System.Collections.Generic;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Music) for Sound Voltex
/// </summary>
public partial class SvMusic
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string TitleYomigana { get; set; } = null!;

    public string Artist { get; set; } = null!;

    public string ArtistYomigana { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int Version { get; set; }

    // asphyxia music_db.info.inf_ver — XCD/infinite version flag. Used by the
    // non-unlock music_limited path to emit music_type=3 (INFINITE) only for
    // songs whose inf_ver matches the running version (asphyxia common.ts L206).
    public int InfVer { get; set; }

    // asphyxia music_db.info.distribution_date (YYYYMMDD int). Used to skip
    // unreleased songs in the non-unlock music_limited path (common.ts L185/L225).
    public int DistributionDate { get; set; }

    public virtual ICollection<SvScore> SvScores { get; } = new List<SvScore>();
}
