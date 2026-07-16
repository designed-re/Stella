using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Server information notices per version (asphyxia INFORMATION6/INFORMATION7).
/// </summary>
public partial class SvInformationData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int InfoId { get; set; }

    public string InfoStr { get; set; } = null!;

    public int MinVersion { get; set; }

    public int StartDate { get; set; }
}