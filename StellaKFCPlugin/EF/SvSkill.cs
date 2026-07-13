using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Skill) for Sound Voltex - per-version skill analyzer selection.
/// </summary>
public partial class SvSkill
{
    public int Id { get; set; }

    public int Profile { get; set; }

    public int Version { get; set; }

    public short Base { get; set; }

    public short Level { get; set; }

    public short Name { get; set; }

    public short Type { get; set; }

    public virtual SvProfile ProfileNavigation { get; set; } = null!;
}