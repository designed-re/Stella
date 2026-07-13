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
}