using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Stella.Abstractions.Plugins;

namespace StellaKFCPlugin
{
    public class StellaKFCPluginConfig : IStellaPluginConfig
    {
        [ConfigurationKeyName("db")]
        [JsonPropertyName("db")]
        public string DbConnectionString { get; set; }

        [ConfigurationKeyName("enabled")]
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [ConfigurationKeyName("maintenance")]
        [JsonPropertyName("maintenance")]
        public bool MaintenanceMode { get; set; }

        [ConfigurationKeyName("unlock_all_songs")]
        [JsonPropertyName("unlock_all_songs")]
        public bool UnlockAllSongs { get; set; }

        [ConfigurationKeyName("arena_open")]
        [JsonPropertyName("arena_open")]
        public bool ArenaOpen { get; set; }

        [ConfigurationKeyName("arena_session")]
        [JsonPropertyName("arena_session")]
        public int ArenaSession { get; set; }

        [ConfigurationKeyName("arena_station")]
        [JsonPropertyName("arena_station")]
        public string? ArenaStation { get; set; }

        [ConfigurationKeyName("unlock_all_navigators")]
        [JsonPropertyName("unlock_all_navigators")]
        public bool UnlockAllNavigators { get; set; }

        [ConfigurationKeyName("unlock_all_appeal_cards")]
        [JsonPropertyName("unlock_all_appeal_cards")]
        public bool UnlockAllAppealCards { get; set; }

        [ConfigurationKeyName("unlock_all_valk_items")]
        [JsonPropertyName("unlock_all_valk_items")]
        public bool UnlockAllValkItems { get; set; }

        [ConfigurationKeyName("use_blasterpass")]
        [JsonPropertyName("use_blasterpass")]
        public bool UseBlasterPass { get; set; } = true;

        [ConfigurationKeyName("arena_no_endtime")]
        [JsonPropertyName("arena_no_endtime")]
        public bool ArenaNoEndtime { get; set; } = true;

        // GRAVITY WARS (sv3) options
        [ConfigurationKeyName("gw_mission")]
        [JsonPropertyName("gw_mission")]
        public bool GwMission { get; set; }

        [ConfigurationKeyName("gw_mission_skipmatch")]
        [JsonPropertyName("gw_mission_skipmatch")]
        public bool GwMissionSkipmatch { get; set; }

        [ConfigurationKeyName("gw_gene")]
        [JsonPropertyName("gw_gene")]
        public bool GwGene { get; set; } = true;
    }
}

