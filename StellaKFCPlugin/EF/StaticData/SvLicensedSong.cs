using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Licensed songs per version (asphyxia LICENSED_SONGS6/7).
/// </summary>
public partial class SvLicensedSong
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int MusicId { get; set; }
}