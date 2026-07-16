using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Apigene (API Valkyrie Generator) info - NABLA only (asphyxia APIGENE7.info).
/// </summary>
public partial class SvApigeneData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int ApigeneId { get; set; }

    public string Name { get; set; } = null!;

    public string NameEnglish { get; set; } = null!;

    public int CommonRate { get; set; }

    public int UncommonRate { get; set; }

    public int RareRate { get; set; }

    public int Price { get; set; }

    public bool NoDuplicate { get; set; }

    public int MinVersion { get; set; }
}