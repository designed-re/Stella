using CorePlugin.EF;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.Plugins;

namespace CorePlugin
{
    public class CorePlugin : IStellaPlugin
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
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(),"plugins","plugin_core.json")).Build();
            var coreConfig = config.Get<CorePluginConfig>();
            PluginConfig = coreConfig;
          builder.Services.AddDbContext<CoreContext>(x => x.UseMySql(coreConfig.DbConnectionString,
               new MariaDbServerVersion(ServerVersion.AutoDetect(coreConfig.DbConnectionString))));
            return Task.CompletedTask;
        }

        public Task OnAppInitialize(WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CoreContext>())
            {
                context.Database.Migrate();

                context.Database.EnsureCreated();
            }
            return Task.CompletedTask;
        }
    }
}


