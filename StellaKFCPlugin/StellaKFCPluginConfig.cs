using Microsoft.Extensions.Configuration;
using Stella.Abstractions.Plugins;

namespace StellaKFCPlugin
{
    internal class StellaKFCPluginConfig : IStellaPluginConfig
    {
        [ConfigurationKeyName("db")]
        public string DbConnectionString { get; set; }

        [ConfigurationKeyName("enabled")]
        public bool Enabled { get; set; }

        [ConfigurationKeyName("maintenance")]
        public bool MaintenanceMode { get; set; }

        [ConfigurationKeyName("unlock_all_songs")]
        public bool UnlockAllSongs { get; set; }

        [ConfigurationKeyName("arena_open")]
        public bool ArenaOpen { get; set; }

        [ConfigurationKeyName("arena_session")]
        public int ArenaSession { get; set; }

        // TODO: Add KFC-specific configuration properties here
        // Example:
        // [ConfigurationKeyName("game_mode")]
        // public string GameMode { get; set; }
    }
}

