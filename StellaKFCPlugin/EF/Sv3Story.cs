namespace StellaKFCPlugin.EF;

/// <summary>
/// GRAVITY WARS (sv3) story progression data. Ported from asphyxia
/// kfc/models/gw_story.ts.
/// </summary>
public class Sv3Story
{
    public int Id { get; set; }

    public string RefId { get; set; } = null!;

    public int Version { get; set; }

    public int StoryId { get; set; }

    public int ProgressId { get; set; }

    public int ProgressParam { get; set; }

    public int ClearCnt { get; set; }

    public uint RouteFlg { get; set; }
}
