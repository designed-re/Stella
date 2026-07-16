using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Policy Break) for Sound Voltex - infinite infection era.
/// </summary>
public partial class SvPolicyBreak
{
    public int Id { get; set; }

    public string RefId { get; set; } = null!;

    public int Version { get; set; }

    public int Id1 { get; set; }

    public int Exp { get; set; }
}