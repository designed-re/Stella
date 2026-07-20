using System;
using System.Linq;

namespace Stella.Abstractions.Cards;

/// <summary>Process-wide registry for the single <see cref="IStellaCardProvider"/>.</summary>
public static class StellaCardProviderRegistry
{
    private static IStellaCardProvider? _provider;
    private static readonly object _lock = new();

    public static void Register(IStellaCardProvider provider)
    {
        lock (_lock) { _provider = provider; }
    }

    public static IStellaCardProvider? Current
    {
        get { lock (_lock) { return _provider; } }
    }
}
