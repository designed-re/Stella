using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Data.Seed;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Handlers;

namespace StellaKFCPlugin
{
    public class StellaKFCPlugin : IStellaPlugin
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
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "plugins", "plugin_kfc.json"), optional: true)
                .Build();

            var kfcConfig = config.Get<StellaKFCPluginConfig>() ?? new StellaKFCPluginConfig();

            // Allow overriding the DB connection string via the STELLA_KFC_DB
            // environment variable so credentials do not have to be committed in
            // plugin_kfc.json.
            var envConn = Environment.GetEnvironmentVariable("STELLA_KFC_DB");
            if (!string.IsNullOrWhiteSpace(envConn))
                kfcConfig.DbConnectionString = envConn;

            PluginConfig = kfcConfig;

            var (connStr, serverVersion) = StellaKFCContext.ResolveConfiguration();
            builder.Services.AddDbContext<StellaKFCContext>(x => x.UseMySql(connStr, serverVersion));
            return Task.CompletedTask;
        }

        public Task OnAppInitialize(WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<StellaKFCContext>())
            {
                context.Database.Migrate();

                // Seed static game data (events, courses, valgene, ...) from
                // Data/Seed/asphyxia_data.json if the tables are empty. Idempotent.
                try
                {
                    KfcSeeder.Seed(context);
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetService<ILogger<StellaKFCPlugin>>();
                    logger?.LogWarning(ex, "KfcSeeder failed; static data may be incomplete.");
                }

                // Load music_db.xml (shift_jis) into SvMusic once so the ViiMigrate
                // volforce computation and common music_limited logic can query difficulty.
                try
                {
                    var logger = app.Services.GetService<ILogger<StellaKFCPlugin>>();
                    MigrationHelper.LoadMusicDbAsync(context, logger).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetService<ILogger<StellaKFCPlugin>>();
                    logger?.LogWarning(ex, "LoadMusicDbAsync failed; SvMusic may be empty.");
                }
            }
            return Task.CompletedTask;
        }
    }
}
