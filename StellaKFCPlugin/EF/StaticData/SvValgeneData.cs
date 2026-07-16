using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Valkyrie Generator (gacha) info per version (asphyxia VALGENE.info / VALGENE7.info).
/// </summary>
public partial class SvValgeneData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int ValgeneId { get; set; }

    public string ValgeneName { get; set; } = null!;

    public string ValgeneNameEnglish { get; set; } = null!;

    public int MinVersion { get; set; }
}