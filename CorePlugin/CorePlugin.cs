using CorePlugin.EF;
using Microsoft.EntityFrameworkCore;
using Stella.Abstractions.Cards;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.Plugins;

namespace CorePlugin
{
    public class CorePlugin : IStellaPlugin, IStellaCardProvider
    {
        public string Name => "CorePlugin";
        public string Version => "1.0.0";
        public string Description => "Core plugin for EAMUSE protocol";
        public string GameCode => "STELLA_PROTOCOL";
        public int? MinVer => null;
        public int? MaxVer => null;
        public IStellaPluginConfig PluginConfig { get; set; }

        public Task OnBuilderInitialize(WebApplicationBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(CoreContext.ResolvePluginConfigPath("plugin_core.json"), optional: true)
                .Build();
            var coreConfig = config.Get<CorePluginConfig>() ?? new CorePluginConfig();

            // DB connection string comes solely from plugin_core.json (no env vars).
            PluginConfig = coreConfig;
            StellaCardProviderRegistry.Register(this);

            var (connStr, serverVersion) = CoreContext.ResolveConfiguration();
            builder.Services.AddDbContext<CoreContext>(x => x.UseMySql(connStr, serverVersion));
            return Task.CompletedTask;
        }

        public Task OnAppInitialize(WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CoreContext>())
            {
                context.Database.Migrate();

                // context.Database.EnsureCreated();
            }
            return Task.CompletedTask;
        }

        public async Task<WebUICard?> GetCardAsync(string refid)
        {
            using var context = new CoreContext();
            var card = await context.Cards.FirstOrDefaultAsync(c => c.RefId == refid);
            return card is null ? null : new WebUICard
            {
                RefId = card.RefId,
                CardId = card.CardId,
                Paseli = card.Paseli,
                PassCode = card.PassCode,
            };
        }
    }
}


