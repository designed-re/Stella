using Microsoft.EntityFrameworkCore;

namespace Stella.MigrationHelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // The connection string is resolved via StellaKFCContext.ResolveConfiguration(),
            // which prefers the STELLA_KFC_DB environment variable and falls back to
            // plugins/plugin_kfc.json. Avoid hardcoding credentials here.
            var (kfcConnStr, kfcServerVersion) = StellaKFCPlugin.EF.StellaKFCContext.ResolveConfiguration();
            builder.Services.AddDbContext<StellaKFCPlugin.EF.StellaKFCContext>(x => x.UseMySql(kfcConnStr, kfcServerVersion));
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
