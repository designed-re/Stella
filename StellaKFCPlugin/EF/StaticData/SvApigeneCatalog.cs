using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Apigene catalog items - NABLA only (asphyxia APIGENE7.catalog).
/// </summary>
public partial class SvApigeneCatalog
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int ApigeneId { get; set; }

    public int ItemType { get; set; }

    public int ItemId { get; set; }

    public int Rarity { get; set; }
}