using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CorePlugin.EF
{
    public class CoreContext : DbContext
    {
        internal DbSet<Card> Cards { get; set; }
        internal DbSet<Facility> Facilities { get; set; }

        private static string? _cachedConnectionString;
        private static MariaDbServerVersion? _cachedServerVersion;
        private static readonly object _configLock = new();

        public CoreContext()
        {
        }

        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        /// <summary>
        /// Resolves the Core DB connection string once (caching the MariaDB server
        /// version so <c>new CoreContext()</c> does not re-read config / re-detect the
        /// server version on every request). The connection string comes solely from
        /// <c>plugins/plugin_core.json</c> (no env vars).
        /// </summary>
        public static (string ConnectionString, MariaDbServerVersion ServerVersion) ResolveConfiguration()
        {
            if (_cachedConnectionString is not null)
                return (_cachedConnectionString, _cachedServerVersion!);

            lock (_configLock)
            {
                if (_cachedConnectionString is not null)
                    return (_cachedConnectionString, _cachedServerVersion!);

                var config = new ConfigurationBuilder()
                    .AddJsonFile(ResolvePluginConfigPath("plugin_core.json"), optional: true)
                    .Build();
                var coreConfig = config.Get<CorePluginConfig>() ?? new CorePluginConfig();

                var connStr = coreConfig.DbConnectionString;
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new InvalidOperationException(
                        "CorePlugin DB connection string is not configured. Set the \"db\" field in plugins/plugin_core.json.");

                _cachedConnectionString = connStr;
                _cachedServerVersion = new MariaDbServerVersion(ServerVersion.AutoDetect(connStr));
                return (_cachedConnectionString, _cachedServerVersion!);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var (connStr, serverVersion) = ResolveConfiguration();
            optionsBuilder.UseMySql(connStr, serverVersion);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

            builder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("card", tb => tb.HasComment("Stores e-amusement cards"));

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.CardId)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("card_id");
                entity.Property(e => e.CardNo)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("card_no");
                entity.Property(e => e.Paseli)
                    .HasColumnType("int(11)")
                    .HasColumnName("paseli")
                    .HasDefaultValue(10000);
                entity.Property(e => e.PaseliSession)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("paseli_session");
                entity.Property(e => e.PassCode)
                    .HasMaxLength(4)
                    .IsFixedLength()
                    .HasColumnName("pass");
                entity.Property(e => e.RefId)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasComment("same with dataid")
                    .HasColumnName("ref_id");
            });

            builder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("facility", tb => tb.HasComment("Stores facility"));

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.PCBId)
                    .HasColumnName("pcb_id");
                entity.Property(e => e.Country)
                    .HasColumnName("country");
                entity.Property(e => e.Region)
                    .HasColumnName("region");
                entity.Property(e => e.Name)
                    .HasColumnName("name");
                entity.Property(e => e.Type)
                    .HasColumnType("int(11)")
                    .HasColumnName("type")
                    .HasDefaultValue(0);
                entity.Property(e => e.CountryName)
                    .HasColumnName("country_name");
                entity.Property(e => e.CountryJName)
                    .HasColumnName("country_jname");
                entity.Property(e => e.RegionName)
                    .HasColumnName("region_name");
                entity.Property(e => e.RegionJName)
                    .HasColumnName("region_jname");
                entity.Property(e => e.CustomerCode)
                    .HasColumnName("customer_code");
                entity.Property(e => e.CompanyCode)
                    .HasColumnName("company_code");
                entity.Property(e => e.FacilityId)
                    .HasColumnName("facility_id");

            });
        }
        /// <summary>
        /// Resolves a plugin config file under plugins/, checking the current
        /// working directory first (production/Docker app root) then the host
        /// assembly base directory (bin output when running via `dotnet run`).
        /// </summary>
        public static string ResolvePluginConfigPath(string fileName)
        {
            var cwd = Path.Combine(Directory.GetCurrentDirectory(), "plugins", fileName);
            if (File.Exists(cwd)) return cwd;
            return Path.Combine(AppContext.BaseDirectory, "plugins", fileName);
        }

    }
}
