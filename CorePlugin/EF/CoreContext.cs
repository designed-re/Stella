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

        public CoreContext()
        {

        }

        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "plugins", "plugin_core.json")).Build();
            var coreConfig = config.Get<CorePluginConfig>();
            optionsBuilder.UseMySql(coreConfig.DbConnectionString,
                new MariaDbServerVersion(ServerVersion.AutoDetect(coreConfig.DbConnectionString)));
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
    }
}
