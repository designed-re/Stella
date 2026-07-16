using System;
using System.Collections.Generic;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Course Records) for Sound Voltex
/// </summary>
public partial class SvCourseRecord
{
    public int Id { get; set; }

    public int Profile { get; set; }

    public short SeriesId { get; set; }

    public short CourseId { get; set; }

    public short SkillType { get; set; }

    public string KacId { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Score { get; set; }

    public int Exscore { get; set; }

    public short Clear { get; set; }

    public short Grade { get; set; }

    public short Rate { get; set; }

    public short Count { get; set; }

    public virtual SvProfile ProfileNavigation { get; set; } = null!;
}
