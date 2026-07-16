using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Skill Analyzer course definition per version (asphyxia COURSES6/COURSES7).
/// </summary>
public partial class SvCourseData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int SeriesId { get; set; }

    public string SeriesName { get; set; } = null!;

    public bool IsNew { get; set; }

    public short HasGod { get; set; }

    /// <summary>Minimum datecode for this season to appear (asphyxia course.version).</summary>
    public int MinVersion { get; set; }

    /// <summary>JSON-serialised CourseElement array: id/type/name/level/nameID/assist/tracks.</summary>
    public string CoursesJson { get; set; } = null!;
}