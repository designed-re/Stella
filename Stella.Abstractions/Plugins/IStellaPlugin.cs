using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Stella.Abstractions.WebUI;

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

        /// <summary>
        /// Top-level WebUI pages contributed by this plugin (asphyxia non-
        /// <c>profile_</c> pug files). The host renders a sidebar entry per
        /// descriptor. Default: none.
        /// </summary>
        IReadOnlyList<WebUIPageDescriptor> WebUIPages => Array.Empty<WebUIPageDescriptor>();

        /// <summary>
        /// Profile-detail tab pages contributed by this plugin (asphyxia
        /// <c>profile_</c> pug files). Default: none.
        /// </summary>
        IReadOnlyList<WebUIPageDescriptor> ProfilePages => Array.Empty<WebUIPageDescriptor>();

        /// <summary>
        /// Registers AJAX event handlers (asphyxia <c>R.WebUIEvent</c>).
        /// Called once during host startup after plugins are loaded. Default: no-op.
        /// </summary>
        void RegisterWebUIEvents(IWebUIEventRouter router) { }

        /// <summary>
        /// Renders a top-level WebUI page (asphyxia non-<c>profile_</c> pug) as an
        /// HTML fragment. The host wraps the fragment in the shared layout. The
        /// plugin resolves its view via <see cref="PluginViewRenderer"/> (from
        /// <paramref name="services"/>). Default: not implemented (null).
        /// </summary>
        Task<string?> RenderWebUIPageAsync(string slug, IServiceProvider services) => Task.FromResult<string?>(null);

        /// <summary>
        /// Renders a profile-tab page (asphyxia <c>profile_</c> pug) as an HTML
        /// fragment. Default: not implemented (null).
        /// </summary>
        Task<string?> RenderProfileTabAsync(string slug, string refid, IServiceProvider services) => Task.FromResult<string?>(null);
    }
}
