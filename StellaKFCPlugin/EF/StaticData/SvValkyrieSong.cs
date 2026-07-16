using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Valkyrie songs (asphyxia VALKYRIE_SONGS).
/// </summary>
public partial class SvValkyrieSong
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int MusicId { get; set; }
}