using Microsoft.EntityFrameworkCore;

namespace Stella.MigrationHelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<StellaKFCPlugin.EF.StellaKFCContext>(x=> x.UseMySql("server=localhost;database=stella;user id=stella;password=stella", new MariaDbServerVersion(ServerVersion.AutoDetect("server=localhost;database=stella;user id=stella;password=stella"))));
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
