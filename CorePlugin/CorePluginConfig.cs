using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Stella.Abstractions.Plugins;

namespace CorePlugin
{
    internal class CorePluginConfig : IStellaPluginConfig
    {
        [ConfigurationKeyName("db")]
        public string DbConnectionString { get; set; }

        [ConfigurationKeyName("maintenance")]
        public bool MaintenanceMode { get; set; }

        [ConfigurationKeyName("enabled")]
        public bool Enabled { get; set; }
        [ConfigurationKeyName("register_mode")]
        public bool RegisterMode { get; set; }
        [ConfigurationKeyName("private_mode")]
        public bool PrivateMode { get; set; }
    }
}
