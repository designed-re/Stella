using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Stella.Abstractions.Plugins;

namespace CorePlugin
{
    internal class CorePluginConfig : IStellaPluginConfig
    {
        [ConfigurationKeyName("db")]
        [JsonPropertyName("db")]
        public string DbConnectionString { get; set; }

        [ConfigurationKeyName("maintenance")]
        [JsonPropertyName("maintenance")]
        public bool MaintenanceMode { get; set; }

        [ConfigurationKeyName("enabled")]
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        [ConfigurationKeyName("register_mode")]
        [JsonPropertyName("register_mode")]
        public bool RegisterMode { get; set; }
        [ConfigurationKeyName("private_mode")]
        [JsonPropertyName("private_mode")]
        public bool PrivateMode { get; set; }
    }
}
