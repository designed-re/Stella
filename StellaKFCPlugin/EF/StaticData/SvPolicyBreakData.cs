using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Policy break definitions (asphyxia POLICY_BREAK2) - infinite infection era.
/// </summary>
public partial class SvPolicyBreakData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int Pbid { get; set; }

    public int RwrdType { get; set; }

    public int RwrdId { get; set; }

    public int RwrdParam { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    /// <summary>Japanese title (from asphyxia titleJ).</summary>
    public string TitleJ { get; set; } = string.Empty;

    /// <summary>English title (from asphyxia titleE).</summary>
    public string TitleE { get; set; } = string.Empty;

    /// <summary>Target game id (from asphyxia tgt).</summary>
    public int TargetId { get; set; }

    /// <summary>Reward point (from asphyxia rwrd.point).</summary>
    public int RwrdPoint { get; set; }

    /// <summary>Reward music id (from asphyxia rwrd.id).</summary>
    public int RwrdMusicId { get; set; }
}