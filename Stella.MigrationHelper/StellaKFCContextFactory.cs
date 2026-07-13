using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StellaKFCPlugin.EF;

namespace Stella.MigrationHelper;

/// <summary>
/// Design-time factory for StellaKFCContext so <c>dotnet ef migrations add</c>
/// works without a live MySQL connection (avoids ServerVersion.AutoDetect).
/// </summary>
public class StellaKFCContextFactory : IDesignTimeDbContextFactory<StellaKFCContext>
{
    public StellaKFCContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("STELLA_KFC_DB");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            connStr = "Server=localhost;Port=3306;User=stella;Password=stella;Database=stella_kfc";
        }

        var serverVersion = new MariaDbServerVersion(new Version(10, 5, 0));

        var options = new DbContextOptionsBuilder<StellaKFCContext>()
            .UseMySql(connStr, serverVersion)
            .Options;

        return new StellaKFCContext(options);
    }
}