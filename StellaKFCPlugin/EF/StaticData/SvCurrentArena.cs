using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Current arena season per version (asphyxia CURRENT_ARENA / CURRENT_ARENA7).
/// </summary>
public partial class SvCurrentArena
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int Season { get; set; }

    public short Rule { get; set; }

    public short RankMatchTarget { get; set; }

    public long TimeStart { get; set; }

    public long TimeEnd { get; set; }

    public long ShopStart { get; set; }

    public long ShopEnd { get; set; }
}