using Microsoft.Extensions.Configuration;
using Stella.Abstractions.Plugins;

namespace StellaKFCPlugin
{
    public class StellaKFCPluginConfig : IStellaPluginConfig
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

        [ConfigurationKeyName("arena_station")]
        public string? ArenaStation { get; set; }

        [ConfigurationKeyName("unlock_all_navigators")]
        public bool UnlockAllNavigators { get; set; }

        [ConfigurationKeyName("unlock_all_appeal_cards")]
        public bool UnlockAllAppealCards { get; set; }

        [ConfigurationKeyName("unlock_all_valk_items")]
        public bool UnlockAllValkItems { get; set; }

        [ConfigurationKeyName("use_blasterpass")]
        public bool UseBlasterPass { get; set; } = true;

        [ConfigurationKeyName("arena_no_endtime")]
        public bool ArenaNoEndtime { get; set; } = true;

        // TODO: Add KFC-specific configuration properties here
        // Example:
        // [ConfigurationKeyName("game_mode")]
        // public string GameMode { get; set; }
    }
}

