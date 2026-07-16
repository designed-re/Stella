using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Extend info per version (asphyxia EXTENDS6/EXTENDS7).
/// </summary>
public partial class SvExtendData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public uint ExtendId { get; set; }

    public uint ExtendType { get; set; }

    public int ParamNum1 { get; set; }

    public int ParamNum2 { get; set; }

    public int ParamNum3 { get; set; }

    public int ParamNum4 { get; set; }

    public int ParamNum5 { get; set; }

    public string ParamStr1 { get; set; } = string.Empty;

    public string ParamStr2 { get; set; } = string.Empty;

    public string ParamStr3 { get; set; } = string.Empty;

    public string ParamStr4 { get; set; } = string.Empty;

    public string ParamStr5 { get; set; } = string.Empty;

    public int MinVersion { get; set; }

    public int StartDate { get; set; }
}