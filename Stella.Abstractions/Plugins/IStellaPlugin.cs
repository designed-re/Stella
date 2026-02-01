using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Stella.Abstractions.Plugins
{
    public interface IStellaPlugin
    {
        public string Name { get; }
        public string Version { get; }
        public string Description { get; }
        public string GameCode { get; }
        public int? MinVer { get; }
        public int? MaxVer { get; }
        public IStellaPluginConfig PluginConfig { get; set; }
        

        Task OnBuilderInitialize(WebApplicationBuilder builder);
        Task OnAppInitialize(WebApplication app);
    }
}
