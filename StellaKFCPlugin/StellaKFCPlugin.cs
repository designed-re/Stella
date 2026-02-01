using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;

namespace StellaKFCPlugin
{
    public class StellaKFCPlugin : IStellaPlugin
    {
        public string Name => "StellaKFCPlugin";
        public string Version => "1.0.0";
        public string Description => "EXCEED";
        public string GameCode => "KFC";
        public int? MinVer => null;
        public int? MaxVer => null;
        public IStellaPluginConfig PluginConfig { get; set; }

        public Task OnBuilderInitialize(WebApplicationBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "plugins", "plugin_kfc.json"))
                .Build();
            
            var kfcConfig = config.Get<StellaKFCPluginConfig>();
            PluginConfig = kfcConfig;

            // TODO: Add DbContext and other services initialization here
            // Example: builder.Services.AddDbContext<KFCContext>(...)
            builder.Services.AddDbContext<StellaKFCContext>(x => x.UseMySql(kfcConfig.DbConnectionString,
                new MariaDbServerVersion(ServerVersion.AutoDetect(kfcConfig.DbConnectionString))));
            return Task.CompletedTask;
        }

        public Task OnAppInitialize(WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<StellaKFCContext>())
            {
                context.Database.Migrate();

                context.Database.EnsureCreated();
            }
            // TODO: Add app initialization logic here
            return Task.CompletedTask;
        }
    }
}
