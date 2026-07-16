using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Valkyrie Generator catalog items per version (asphyxia VALGENE.catalog / VALGENE7.catalog).
/// </summary>
public partial class SvValgeneCatalog
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int ValgeneId { get; set; }

    public int ItemType { get; set; }

    public int ItemId { get; set; }

    public int Rarity { get; set; }
}