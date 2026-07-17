using System;
using System.Collections.Generic;
using System.Linq;

namespace Stella.Abstractions.Plugins
{
    /// <summary>
    /// Process-wide registry of loaded <see cref="IStellaPlugin"/> instances.
    /// Populated by <c>PluginService</c> as plugins load, this lets the core
    /// plugin query game plugins without a direct project reference (mirroring
    /// asphyxia's <c>ROOT_CONTAINER.getPluginByCode</c>).
    /// </summary>
    public static class StellaPluginRegistry
    {
        private static readonly List<IStellaPlugin> _plugins = new();
        private static readonly object _lock = new();

        public static void Register(IStellaPlugin plugin)
        {
            lock (_lock)
            {
                if (!_plugins.Contains(plugin))
                    _plugins.Add(plugin);
            }
        }

        /// <summary>Returns the plugin registered for <paramref name="gameCode"/>, or null.</summary>
        public static IStellaPlugin? GetByGameCode(string? gameCode)
        {
            if (string.IsNullOrEmpty(gameCode)) return null;
            lock (_lock)
            {
                return _plugins.FirstOrDefault(p =>
                    string.Equals(p.GameCode, gameCode, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
