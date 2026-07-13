using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Counter) for Sound Voltex - generic integer counters (e.g. mix id).
/// </summary>
public partial class SvCounter
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public int Value { get; set; }
}