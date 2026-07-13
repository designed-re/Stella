using System.Collections.Generic;

namespace StellaKFCPlugin.Data;

public readonly record struct SvEgSongLockedCategory(string Category, IReadOnlyList<int> MusicIds);