using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Stella.Abstractions.Plugins
{
    public interface IStellaPluginConfig
    {
        [ConfigurationKeyName("db")]
        public string DbConnectionString { get; set; }

        [ConfigurationKeyName("enabled")]
        public bool Enabled { get; set; }
    }
}
