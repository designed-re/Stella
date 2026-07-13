using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// EG songs locked categories for NABLA cross-resonance (asphyxia EGSONGS_LOCKED).
/// </summary>
public partial class SvEgSongLocked
{
    public int Id { get; set; }

    public int Version { get; set; }

    public string Category { get; set; } = null!;

    public int MusicId { get; set; }
}