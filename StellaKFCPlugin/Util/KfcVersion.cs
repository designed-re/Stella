using System;
using System.Globalization;

namespace StellaKFCPlugin.Util;

/// <summary>
/// Version helpers ported from asphyxia kfc/utils.ts. The model string
/// (e.g. <c>KFC:LJG:J:G:2025120900:1</c>) carries the datecode at index 4.
/// </summary>
public static class KfcVersion
{
    /// <summary>
    /// Returns the absolute game version: 1 (BOOTH), 2 (infinite infection),
    /// 6 (EXCEED GEAR), 7 (NABLA). Mirrors asphyxia <c>Math.abs(getVersion(info))</c>.
    /// Stella only supports 6 and 7.
    /// </summary>
    public static int GetVersion(string? model)
    {
        if (string.IsNullOrEmpty(model)) return 0;
        var parts = model.Split(':');
        if (parts.Length < 5) return 0;
        // datecode is like "2025120900" — strip trailing 2-digit suffix for comparison.
        if (!int.TryParse(parts[4].AsSpan(0, Math.Max(0, parts[4].Length - 2)), out var dateCode))
            return 0;
        if (dateCode <= 2013052900) return 1;
        if (dateCode <= 2014112000) return 2;
        if (dateCode <= 2016121200) return 3;
        if (dateCode <= 2021082400) return 6;
        if (dateCode <= 2025121900) return 6; // asphyxia returns -6 (EG post-20210831); abs is 6
        return 7;
    }

    /// <summary>Full datecode integer from the model (e.g. 2025120900).</summary>
    public static int GetDateCode(string? model)
    {
        if (string.IsNullOrEmpty(model)) return 0;
        var parts = model.Split(':');
        if (parts.Length < 5) return 0;
        return int.TryParse(parts[4].AsSpan(0, Math.Max(0, parts[4].Length - 2)), out var d) ? d : 0;
    }

    /// <summary>Cabinet type letter from model (index 2): A/J/G/H/...</summary>
    public static string GetCabType(string? model)
    {
        if (string.IsNullOrEmpty(model)) return "A";
        var parts = model.Split(':');
        return parts.Length > 2 ? parts[2] : "A";
    }

    /// <summary>
    /// asphyxia checkVerStart: returns true when <paramref name="gameVersion"/> is
    /// greater-or-equal to <paramref name="checkVersion"/> AND <paramref name="date"/>
    /// is on-or-after the start date (YYYYMMDD). start=0 means "always".
    /// </summary>
    public static bool CheckVerStart(int gameVersion, int checkVersion, int start, DateTime date)
    {
        if (start == 0) return true;
        int startYr = start / 10000;
        int startMo = (start / 100) % 100;
        int startDa = start % 100;
        var checkStartUtc = new DateTime(startYr, startMo, startDa, 0, 0, 0, DateTimeKind.Utc);
        if (gameVersion < checkVersion) return false;
        if (date.ToUniversalTime() < checkStartUtc) return false;
        return true;
    }

    /// <summary>asphyxia computeForce: EG/NABLA volforce from difficulty/score/medal/grade.</summary>
    public static int ComputeForce(double diff, int score, int medal, int grade)
    {
        double[] medalCoef = { 0, 0.50, 1.0, 1.02, 1.04, 1.05, 1.10 };
        double[] gradeCoef = { 0, 0.8, 0.82, 0.85, 0.88, 0.91, 0.94, 0.97, 1.0, 1.02, 1.05 };
        double m = medal >= 0 && medal < medalCoef.Length ? medalCoef[medal] : 0;
        double g = grade >= 0 && grade < gradeCoef.Length ? gradeCoef[grade] : 0;
        return (int)Math.Floor(diff * (score / 10000000.0) * g * m * 20);
    }

    /// <summary>asphyxia IDToCode: zero-padded 8-digit id formatted as 0000-0000.</summary>
    public static string IdToCode(int id)
    {
        var padded = id.ToString("D8", CultureInfo.InvariantCulture);
        return $"{padded[..4]}-{padded[4..]}";
    }

    /// <summary>Current YMD as integer (e.g. 20260713).</summary>
    public static int CurrentYmd(DateTime date) =>
        int.Parse(date.ToString("yyyyMMdd", CultureInfo.InvariantCulture));

    /// <summary>Unix epoch ms for a DateTime (treated as UTC).</summary>
    public static long UnixMs(DateTime dt) =>
        new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds();
}