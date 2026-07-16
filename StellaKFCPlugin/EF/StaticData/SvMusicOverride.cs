using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Music override per version (asphyxia MUSIC_OVERRIDE6/7).
/// </summary>
public partial class SvMusicOverride
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int MusicId { get; set; }

    public int StartDate { get; set; }

    /// <summary>Music info fields (JSON): label/title_name/etc.</summary>
    public string InfoJson { get; set; } = null!;

    /// <summary>Charts (JSON): nov/adv/exh/inf/mxm/ult difnum etc.</summary>
    public string ChartsJson { get; set; } = null!;
}