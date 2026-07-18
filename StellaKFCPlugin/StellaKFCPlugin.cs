using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using Stella.Abstractions.WebUI;
using StellaKFCPlugin.Data;
using StellaKFCPlugin.Handlers;
using StellaKFCPlugin.WebUI;

namespace StellaKFCPlugin
{
    public class StellaKFCPlugin : IStellaGamePlugin
    {
        public string Name => "StellaKFCPlugin";
        public string Version => "2.0.0";
        public string Description => "Sound Voltex (EXCEED GEAR + NABLA)";
        public string GameCode => "KFC";
        public int? MinVer => null;
        public int? MaxVer => null;
        public IStellaPluginConfig PluginConfig { get; set; }

        public Task OnBuilderInitialize(WebApplicationBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(StellaKFCContext.ResolvePluginConfigPath("plugin_kfc.json"), optional: true)
                .Build();

            var kfcConfig = config.Get<StellaKFCPluginConfig>() ?? new StellaKFCPluginConfig();

            // DB connection string comes solely from plugin_kfc.json (no env vars).
            PluginConfig = kfcConfig;

            var (connStr, serverVersion) = StellaKFCContext.ResolveConfiguration();
            builder.Services.AddDbContext<StellaKFCContext>(x => x.UseMySql(connStr, serverVersion));
            builder.Services.AddScoped<KfcWebUIPageRenderer>();
            return Task.CompletedTask;
        }

        /// <summary>
        /// asphyxia <c>CheckProfile</c>: true when any profile exists for
        /// <paramref name="refid"/>. Used by CorePlugin's <c>cardmng.inquire</c>
        /// to report an accurate <c>binded</c> flag.
        /// </summary>
        // ---- WebUI ----
        public IReadOnlyList<WebUIPageDescriptor> WebUIPages { get; } = new[]
        {
            new WebUIPageDescriptor { Title = "Data", Slug = "data", View = "Data.cshtml", Icon = "database" },
            new WebUIPageDescriptor { Title = "Songs List", Slug = "songs-list", View = "SongsList.cshtml", Icon = "music-note" },
            new WebUIPageDescriptor { Title = "Startup Flags", Slug = "startup-flags", View = "StartupFlags.cshtml", Icon = "flag" },
            new WebUIPageDescriptor { Title = "Unlock Events", Slug = "unlock-events", View = "UnlockEvents.cshtml", Icon = "lock-open" },
            new WebUIPageDescriptor { Title = "Update WebUI Assets", Slug = "update-webui-assets", View = "UpdateWebUIAssets.cshtml", Icon = "upload" },
            new WebUIPageDescriptor { Title = "Weekly Score Attack", Slug = "weekly-score-attack", View = "WeeklyScoreAttack.cshtml", Icon = "calendar-clock" },
        };

        public IReadOnlyList<WebUIPageDescriptor> ProfilePages { get; } = new[]
        {
            new WebUIPageDescriptor { Title = "Detail", Slug = "detail", View = "ProfileDetail.cshtml" },
            new WebUIPageDescriptor { Title = "Score", Slug = "score", View = "ProfileScore.cshtml" },
            new WebUIPageDescriptor { Title = "Skill", Slug = "skill", View = "ProfileSkill.cshtml" },
            new WebUIPageDescriptor { Title = "Achievements", Slug = "achievements", View = "ProfileAchievements.cshtml" },
            new WebUIPageDescriptor { Title = "Rivals", Slug = "rivals", View = "ProfileRivals.cshtml" },
            new WebUIPageDescriptor { Title = "Customization", Slug = "customization", View = "ProfileCustomization.cshtml" },
            new WebUIPageDescriptor { Title = "Valkyrie Generator", Slug = "valkyrie-generator", View = "ProfileValkyrieGenerator.cshtml" },
            new WebUIPageDescriptor { Title = "Premium Generator", Slug = "premium-generator", View = "ProfilePremiumGenerator.cshtml" },
        };

        public void RegisterWebUIEvents(IWebUIEventRouter router)
        {
            KfcWebUIEvents.Register(router);
        }

        public Task<string?> RenderWebUIPageAsync(string slug, IServiceProvider services)
            => services.GetRequiredService<KfcWebUIPageRenderer>().RenderPageAsync(slug, services);

        public Task<string?> RenderProfileTabAsync(string slug, string refid, IServiceProvider services)
            => services.GetRequiredService<KfcWebUIPageRenderer>().RenderProfileTabAsync(slug, refid, services);

                public Task<bool> ProfileExistsAsync(string refid)
        {
            using var db = new StellaKFCContext();
            return Task.FromResult(db.SvProfiles.Any(p => p.RefId == refid));
        }

        public Task<IReadOnlyList<WebUIProfileSummary>> GetProfileSummariesAsync()
        {
            using var db = new StellaKFCContext();
            return Task.FromResult<IReadOnlyList<WebUIProfileSummary>>(
                db.SvProfiles
                    .OrderBy(p => p.Name)
                    .AsEnumerable()
                    .Select(p => new WebUIProfileSummary
                    {
                        RefId = p.RefId,
                        Name = p.Name,
                        Code = p.Code,
                        Version = p.Version,
                        PluginId = Name,
                    })
                    .ToList());
        }

        public Task<WebUIProfileDetail?> GetProfileDetailAsync(string refid)
        {
            using var db = new StellaKFCContext();
            var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid);
            if (p is null) return Task.FromResult<WebUIProfileDetail?>(null);
            return Task.FromResult<WebUIProfileDetail?>(new WebUIProfileDetail
            {
                RefId = p.RefId,
                Name = p.Name,
                Code = p.Code,
                Version = p.Version,
                PluginId = Name,
                Extra = new Dictionary<string, string?>
                {
                    ["Packets"] = p.Packets.ToString(),
                    ["Blocks"] = p.Blocks.ToString(),
                    ["KacId"] = p.KacId,
                    ["AppealId"] = p.AppealId.ToString(),
                    ["SkillLevel"] = p.SkillLevel.ToString(),
                    ["SkillBaseId"] = p.SkillBaseId.ToString(),
                },
            });
        }

        public Task OnAppInitialize(WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<StellaKFCContext>())
            {
                context.Database.Migrate();

                // Static-data seeding (sv_static_* from asphyxia_data.json) and
                // music_db.xml loading are now performed on demand from the KFC
                // WebUI ("Data" page) instead of at startup. See KfcWebUISeeder.
                // context.Database.Migrate() still runs so the schema is ready.
            }
            return Task.CompletedTask;
        }
    }
}
