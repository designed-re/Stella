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

        /// <summary>
        /// Returns true when a profile exists for <paramref name="refid"/> in
        /// this plugin's data store. Mirrors asphyxia
        /// <c>CheckProfile(gameCode, refid)</c> used by <c>cardmng.inquire</c> to
        /// report an accurate <c>binded</c> flag. Game plugins override this;
        /// the default (core/non-game plugins) is <c>false</c>.
        /// </summary>
        Task<bool> ProfileExistsAsync(string refid) => Task.FromResult(false);
    }
}
