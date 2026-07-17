using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace StellaKFCPlugin.EF
{
    public class StellaKFCContext : DbContext
    {
        public virtual DbSet<SvItem> SvItems { get; set; }

        public virtual DbSet<SvMusic> SvMusics { get; set; }

        public virtual DbSet<SvParam> SvParams { get; set; }

        public virtual DbSet<SvProfile> SvProfiles { get; set; }

        public virtual DbSet<SvScore> SvScores { get; set; }

        public virtual DbSet<SvEvent> SvEvents { get; set; }

        public virtual DbSet<SvRival> SvRivals { get; set; }

        public virtual DbSet<SvCourseRecord> SvCourseRecords { get; set; }

        public virtual DbSet<SvValgeneTicket> SvValgeneTickets { get; set; }

        public virtual DbSet<SvMatchmaker> SvMatchmakers { get; set; }

        public virtual DbSet<SvArena> SvArenas { get; set; }

        public virtual DbSet<SvVariantPower> SvVariantPowers { get; set; }

        public virtual DbSet<SvSkill> SvSkills { get; set; }

        public virtual DbSet<SvWeeklyMusicScore> SvWeeklyMusicScores { get; set; }

        public virtual DbSet<SvPolicyBreak> SvPolicyBreaks { get; set; }

        public virtual DbSet<SvCounter> SvCounters { get; set; }

        public virtual DbSet<StaticData.SvEventData> SvEventDatas { get; set; }

        public virtual DbSet<StaticData.SvCourseData> SvCourseDatas { get; set; }

        public virtual DbSet<StaticData.SvValgeneData> SvValgeneDatas { get; set; }

        public virtual DbSet<StaticData.SvValgeneCatalog> SvValgeneCatalogs { get; set; }

        public virtual DbSet<StaticData.SvApigeneData> SvApigeneDatas { get; set; }

        public virtual DbSet<StaticData.SvApigeneCatalog> SvApigeneCatalogs { get; set; }

        public virtual DbSet<StaticData.SvExtendData> SvExtendDatas { get; set; }

        public virtual DbSet<StaticData.SvInformationData> SvInformationDatas { get; set; }

        public virtual DbSet<StaticData.SvUnlockEventData> SvUnlockEventDatas { get; set; }

        public virtual DbSet<StaticData.SvArenaStationItem> SvArenaStationItems { get; set; }

        public virtual DbSet<StaticData.SvCurrentArena> SvCurrentArenas { get; set; }

        public virtual DbSet<StaticData.SvMusicOverride> SvMusicOverrides { get; set; }

        public virtual DbSet<StaticData.SvLicensedSong> SvLicensedSongs { get; set; }

        public virtual DbSet<StaticData.SvEgSongLocked> SvEgSongLockeds { get; set; }

        public virtual DbSet<StaticData.SvMegamixData> SvMegamixDatas { get; set; }

        public virtual DbSet<StaticData.SvAprilFoolsSong> SvAprilFoolsSongs { get; set; }

        public virtual DbSet<StaticData.SvValkyrieSong> SvValkyrieSongs { get; set; }

        public virtual DbSet<StaticData.SvPolicyBreakData> SvPolicyBreakDatas { get; set; }

        public virtual DbSet<StaticData.SvWeeklyMusic> SvWeeklyMusics { get; set; }

        public virtual DbSet<StaticData.SvHaveNote> SvHaveNotes { get; set; }
        private static string? _cachedConnectionString;
        private static MariaDbServerVersion? _cachedServerVersion;
        private static readonly object _configLock = new();

        public StellaKFCContext()
        {
        }

        public StellaKFCContext(DbContextOptions<StellaKFCContext> options) : base(options)
        {
        }

        /// <summary>
        /// Resolves the KFC DB connection string once (caching the MariaDB server
        /// version so <c>new StellaKFCContext()</c> does not re-read config / re-detect
        /// the server version on every request). The connection string can be
        /// overridden through the <c>STELLA_KFC_DB</c> environment variable to avoid
        /// committing credentials; otherwise <c>plugins/plugin_kfc.json</c> is used.
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
                    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "plugins", "plugin_kfc.json"), optional: true)
                    .Build();
                var coreConfig = config.Get<StellaKFCPluginConfig>() ?? new StellaKFCPluginConfig();

                var connStr = Environment.GetEnvironmentVariable("STELLA_KFC_DB");
                if (string.IsNullOrWhiteSpace(connStr))
                    connStr = coreConfig.DbConnectionString;
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new InvalidOperationException(
                        "StellaKFCPlugin DB connection string is not configured. Set the STELLA_KFC_DB environment variable or plugins/plugin_kfc.json.");

                _cachedConnectionString = connStr;
                _cachedServerVersion = new MariaDbServerVersion(ServerVersion.AutoDetect(connStr));
                return (_cachedConnectionString, _cachedServerVersion!);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // If options were already configured (e.g. via design-time factory or
            // DI), do not attempt to re-resolve the connection string.
            if (optionsBuilder.IsConfigured)
                return;

            var (connStr, serverVersion) = ResolveConfiguration();
            optionsBuilder.UseMySql(connStr, serverVersion);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

            builder.Entity<SvItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_items", tb => tb.HasComment("Data store(Items) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_profile_to_card(id)");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.ItemId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("item_id");
                entity.Property(e => e.Param)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("param");
                entity.Property(e => e.Profile)
                    .HasColumnType("int(11)")
                    .HasColumnName("profile");
                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("type");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvItems)
                    .HasForeignKey(d => d.Profile)
                    .HasConstraintName("FK_profile_to_card(id)");
            });

            builder.Entity<SvMusic>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_music", tb => tb.HasComment("Data store(Music) for Sound Voltex"));

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Artist)
                    .HasMaxLength(200)
                    .HasColumnName("artist");
                entity.Property(e => e.ArtistYomigana)
                    .HasMaxLength(200)
                    .HasColumnName("artist_yomigana");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.Title)
                    .HasMaxLength(200)
                    .HasColumnName("title");
                entity.Property(e => e.TitleYomigana)
                    .HasMaxLength(200)
                    .HasColumnName("title_yomigana");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);
            });

            builder.Entity<SvParam>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_params", tb => tb.HasComment("Data store(Params) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_param_profile_to_profile(id)");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Param)
                    .HasMaxLength(-1)
                    .HasColumnName("param");
                entity.Property(e => e.ParamCount)
                    .HasColumnType("int(11) unsigned")
                    .HasColumnName("param_count");
                entity.Property(e => e.ParamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("param_id");
                entity.Property(e => e.Profile)
                    .HasColumnType("int(11)")
                    .HasColumnName("profile");
                entity.Property(e => e.Type)
                    .HasColumnType("int(11)")
                    .HasColumnName("type");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvParams)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_param_profile_to_profile(id)");
            });

            builder.Entity<SvProfile>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_profile", tb => tb.HasComment("Data store(Profile) for Sound Voltex"));

                entity.HasIndex(e => e.RefId, "refid");
                entity.HasIndex(e => new { e.RefId, e.Version }, "idx_refid_version").IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.AppealId)
                    .HasColumnType("smallint(5) unsigned")
                    .HasColumnName("appeal_id");
                entity.Property(e => e.ArsOption)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("ars_option");
                entity.Property(e => e.Bgm)
                    .HasColumnType("int(11)")
                    .HasColumnName("bgm");
                entity.Property(e => e.BlasterCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("blaster_count");
                entity.Property(e => e.BlasterEnergy)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("blaster_energy");
                entity.Property(e => e.BlasterPassEnable)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("blaster_pass_enable");
                entity.Property(e => e.BlasterPassLimitDate)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("blaster_pass_limit_date");
                entity.Property(e => e.RefId)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("refid");
                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("code");
                entity.Property(e => e.DayCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("day_count");
                entity.Property(e => e.DrawAdjust)
                    .HasColumnType("int(11)")
                    .HasColumnName("draw_adjust");
                entity.Property(e => e.EarlyLateDisp)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("early_late_disp");
                entity.Property(e => e.EffCLeft)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("eff_c_left");
                entity.Property(e => e.EffCRight)
                    .HasDefaultValueSql("'1'")
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("eff_c_right");
                entity.Property(e => e.ExtrackEnergy)
                    .HasColumnType("smallint(5) unsigned")
                    .HasColumnName("extrack_energy");
                entity.Property(e => e.GaugeOption)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("gauge_option");
                entity.Property(e => e.Headphone)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("headphone");
                entity.Property(e => e.Hispeed)
                    .HasColumnType("int(11)")
                    .HasColumnName("hispeed");
                entity.Property(e => e.KacId)
                    .HasMaxLength(8)
                    .HasDefaultValueSql("'VOLTEX'")
                    .HasColumnName("kac_id");
                entity.Property(e => e.Lanespeed)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("lanespeed");
                entity.Property(e => e.LastMusicId)
                    .HasColumnType("int(11)")
                    .HasColumnName("last_music_id");
                entity.Property(e => e.LastMusicType)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("last_music_type");
                entity.Property(e => e.MaxPlayChain)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("max_play_chain");
                entity.Property(e => e.MaxWeekChain)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("max_week_chain");
                entity.Property(e => e.Name)
                    .HasMaxLength(8)
                    .HasDefaultValueSql("'VOLTEX'")
                    .HasColumnName("name");
                entity.Property(e => e.Nemsys)
                    .HasColumnType("int(11)")
                    .HasColumnName("nemsys");
                entity.Property(e => e.NotesOption)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("notes_option");
                entity.Property(e => e.Pcb)
                    .HasComment("equals with block_no")
                    .HasColumnType("int(11)")
                    .HasColumnName("pcb");
                entity.Property(e => e.Packets)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("packets")
                    .HasDefaultValue(10000);
                entity.Property(e => e.Blocks)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("blocks")
                    .HasDefaultValue(10000);
                entity.Property(e => e.PlayChain)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("play_chain");
                entity.Property(e => e.PlayCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("play_count");
                entity.Property(e => e.SkillBaseId)
                    .HasColumnType("smallint(6)")
                    .HasColumnName("skill_base_id");
                entity.Property(e => e.SkillLevel)
                    .HasColumnType("smallint(6)")
                    .HasColumnName("skill_level");
                entity.Property(e => e.SkillNameId)
                    .HasColumnType("smallint(6)")
                    .HasColumnName("skill_name_id");
                entity.Property(e => e.SortType)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("sort_type");
                entity.Property(e => e.StampA)
                    .HasColumnType("int(11)")
                    .HasColumnName("stampA");
                entity.Property(e => e.StampB)
                    .HasColumnType("int(11)")
                    .HasColumnName("stampB");
                entity.Property(e => e.StampC)
                    .HasColumnType("int(11)")
                    .HasColumnName("stampC");
                entity.Property(e => e.StampD)
                    .HasColumnType("int(11)")
                    .HasColumnName("stampD");
                entity.Property(e => e.SubBg)
                    .HasColumnType("int(11)")
                    .HasColumnName("sub_bg");
                entity.Property(e => e.TodayCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("today_count");
                entity.Property(e => e.WeekChain)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("week_chain");
                entity.Property(e => e.WeekCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("week_count");
                entity.Property(e => e.WeekPlayCount)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("week_play_count");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);

                entity.Property(e => e.Akaname)
                    .HasColumnType("int(11)")
                    .HasColumnName("akaname")
                    .HasDefaultValue(0);
                entity.Property(e => e.BplSupport)
                    .HasColumnType("int(11)")
                    .HasColumnName("bpl_support")
                    .HasDefaultValue(0);
                entity.Property(e => e.CreatorItem)
                    .HasColumnType("int(11)")
                    .HasColumnName("creator_item")
                    .HasDefaultValue(0);
                entity.Property(e => e.Datecode)
                    .HasColumnType("int(11)")
                    .HasColumnName("datecode")
                    .HasDefaultValue(0);
                entity.Property(e => e.PluginVer)
                    .HasColumnType("int(11)")
                    .HasColumnName("plugin_ver")
                    .HasDefaultValue(0);
                entity.Property(e => e.DbVer)
                    .HasColumnType("int(11)")
                    .HasColumnName("dbver")
                    .HasDefaultValue(0);
            });

            builder.Entity<SvScore>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_scores", tb => tb.HasComment("Data store(Scores) for Sound Voltex"));

                entity.HasIndex(e => e.MusicId, "FK_musicid_to_music(id)");

                entity.HasIndex(e => e.Profile, "FK_profile_to_profile(id)");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.ButtonRate)
                    .HasColumnType("int(11)")
                    .HasColumnName("buttonRate");
                entity.Property(e => e.Clear)
                    .HasColumnType("int(11)")
                    .HasColumnName("clear");
                entity.Property(e => e.Exscore)
                    .HasColumnType("int(11)")
                    .HasColumnName("exscore");
                entity.Property(e => e.Grade)
                    .HasColumnType("int(11)")
                    .HasColumnName("grade");
                entity.Property(e => e.LongRate)
                    .HasColumnType("int(11)")
                    .HasColumnName("longRate");
                entity.Property(e => e.MusicId)
                    .HasColumnType("int(11)")
                    .HasColumnName("music_id");
                entity.Property(e => e.Profile)
                    .HasColumnType("int(11)")
                    .HasColumnName("profile");
                entity.Property(e => e.Score)
                    .HasColumnType("int(11)")
                    .HasColumnName("score");
                entity.Property(e => e.Type)
                    .HasColumnType("int(11)")
                    .HasColumnName("type");
                entity.Property(e => e.VolRate)
                    .HasColumnType("int(11)")
                    .HasColumnName("volRate");
                entity.Property(e => e.Volforce)
                    .HasColumnType("int(11)")
                    .HasColumnName("volforce")
                    .HasDefaultValue(0);
                entity.Property(e => e.PlayCount)
                    .HasColumnType("int(11)")
                    .HasColumnName("play_count")
                    .HasDefaultValue(0);
                entity.Property(e => e.DbVer)
                    .HasColumnType("int(11)")
                    .HasColumnName("dbver")
                    .HasDefaultValue(0);
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);

                entity.HasOne(d => d.Music).WithMany(p => p.SvScores)
                    .HasForeignKey(d => d.MusicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_musicid_to_music(id)");

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvScores)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_profile_to_profile(id)");
            });

            builder.Entity<SvEvent>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_events", tb => tb.HasComment("Data store(Events) for Sound Voltex"));

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Event)
                    .HasColumnType("longtext")
                    .HasColumnName("event");
                entity.Property(e => e.Enabled)
                    .HasColumnType("tinyint(1)")
                    .HasColumnName("enabled");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);
            });

            builder.Entity<SvRival>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_rivals", tb => tb.HasComment("Data store(Rivals) for Sound Voltex"));

                entity.HasIndex(e => new { e.RefId, e.Version }, "idx_refid_version");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.RefId)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("ref_id");
                entity.Property(e => e.RivalRefId)
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasColumnName("rival_ref_id");
                entity.Property(e => e.SdvxId)
                    .HasColumnType("int(11)")
                    .HasColumnName("sdvx_id");
                entity.Property(e => e.Name)
                    .HasMaxLength(8)
                    .HasColumnName("name");
                entity.Property(e => e.Mutual)
                    .HasColumnType("tinyint(1)")
                    .HasColumnName("mutual");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);
            });

            builder.Entity<SvCourseRecord>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_course_records", tb => tb.HasComment("Data store(Course Records) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_course_profile_to_profile(id)");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Profile)
                    .HasColumnType("int(11)")
                    .HasColumnName("profile");
                entity.Property(e => e.SeriesId)
                    .HasColumnType("int(11)")
                    .HasColumnName("series_id");
                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("course_id");
                entity.Property(e => e.SkillType)
                    .HasColumnType("smallint(6)")
                    .HasColumnName("skill_type")
                    .HasDefaultValue((short)0);
                entity.Property(e => e.KacId)
                    .HasMaxLength(16)
                    .HasColumnName("kac_id")
                    .HasDefaultValueSql("''");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version")
                    .HasDefaultValue(6);
                entity.Property(e => e.Score)
                    .HasColumnType("int(11)")
                    .HasColumnName("score");
                entity.Property(e => e.Exscore)
                    .HasColumnType("int(11)")
                    .HasColumnName("exscore")
                    .HasDefaultValue(0);
                entity.Property(e => e.Clear)
                    .HasColumnType("int(11)")
                    .HasColumnName("clear");
                entity.Property(e => e.Grade)
                    .HasColumnType("int(11)")
                    .HasColumnName("grade");
                entity.Property(e => e.Rate)
                    .HasColumnType("int(11)")
                    .HasColumnName("rate");
                entity.Property(e => e.Count)
                    .HasColumnType("int(11)")
                    .HasColumnName("count");

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvCourseRecords)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_course_profile_to_profile(id)");
            });

            builder.Entity<SvValgeneTicket>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_valgene_tickets", tb => tb.HasComment("Data store(Valgene Tickets) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_valgene_profile_to_profile(id)");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Profile)
                    .HasColumnType("int(11)")
                    .HasColumnName("profile");
                entity.Property(e => e.TicketNum)
                    .HasColumnType("int(11)")
                    .HasColumnName("ticket_num");
                entity.Property(e => e.LimitDate)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("limit_date");

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvValgeneTickets)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_valgene_profile_to_profile(id)");
            });

            builder.Entity<SvMatchmaker>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_matchmakers", tb => tb.HasComment("Data store(Matchmaker) for Sound Voltex global matching"));

                entity.HasIndex(e => new { e.Version, e.Timestamp }, "idx_version_timestamp");
                entity.HasIndex(e => new { e.Version, e.CVersion, e.Filter, e.Claim, e.EntryId }, "idx_matchmaker_search");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");
                entity.Property(e => e.Version)
                    .HasColumnType("int(11)")
                    .HasColumnName("version");
                entity.Property(e => e.Timestamp)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("timestamp");
                entity.Property(e => e.CVersion)
                    .HasColumnType("int(11)")
                    .HasColumnName("c_version");
                entity.Property(e => e.PlayerNum)
                    .HasColumnType("int(11)")
                    .HasColumnName("player_num");
                entity.Property(e => e.PlayerRemaining)
                    .HasColumnType("int(11)")
                    .HasColumnName("player_remaining");
                entity.Property(e => e.Filter)
                    .HasColumnType("int(11)")
                    .HasColumnName("filter");
                entity.Property(e => e.MusicId)
                    .HasColumnType("int(11)")
                    .HasColumnName("music_id");
                entity.Property(e => e.Seconds)
                    .HasColumnType("int(11)")
                    .HasColumnName("seconds");
                entity.Property(e => e.Port)
                    .HasColumnType("int(11)")
                    .HasColumnName("port");
                entity.Property(e => e.GlobalIp)
                    .HasMaxLength(15)
                    .HasColumnName("global_ip");
                entity.Property(e => e.LocalIp)
                    .HasMaxLength(15)
                    .HasColumnName("local_ip");
                entity.Property(e => e.Claim)
                    .HasColumnType("int(11)")
                    .HasColumnName("claim");
                entity.Property(e => e.EntryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("entry_id");
            });

            builder.Entity<SvArena>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_arena", tb => tb.HasComment("Data store(Arena) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_arena_profile_to_profile(id)");
                entity.HasIndex(e => new { e.Profile, e.Season, e.Version }, "idx_profile_season_version");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Profile).HasColumnType("int(11)").HasColumnName("profile");
                entity.Property(e => e.Season).HasColumnType("int(11)").HasColumnName("season");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version").HasDefaultValue(6);
                entity.Property(e => e.RankPoint).HasColumnType("int(11)").HasColumnName("rank_point").HasDefaultValue(0);
                entity.Property(e => e.ShopPoint).HasColumnType("int(11)").HasColumnName("shop_point").HasDefaultValue(0);
                entity.Property(e => e.UltimateRate).HasColumnType("int(11)").HasColumnName("ultimate_rate").HasDefaultValue(0);
                entity.Property(e => e.UltimateRankNum).HasColumnType("int(11)").HasColumnName("ultimate_rank_num").HasDefaultValue(0);
                entity.Property(e => e.MegamixRate).HasColumnType("int(11)").HasColumnName("megamix_rate").HasDefaultValue(0);
                entity.Property(e => e.RankCount).HasColumnType("int(11)").HasColumnName("rank_count").HasDefaultValue(0);
                entity.Property(e => e.UltimateCount).HasColumnType("int(11)").HasColumnName("ultimate_count").HasDefaultValue(0);
                entity.Property(e => e.LiveEnergy).HasColumnType("int(11)").HasColumnName("live_energy").HasDefaultValue(0);

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvArenas)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_arena_profile_to_profile(id)");
            });

            builder.Entity<SvVariantPower>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_variant_power", tb => tb.HasComment("Data store(Variant Power) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_variant_profile_to_profile(id)");
                entity.HasIndex(e => new { e.Profile, e.Version }, "idx_profile_version");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Profile).HasColumnType("int(11)").HasColumnName("profile");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version").HasDefaultValue(6);
                entity.Property(e => e.Power).HasColumnType("int(11)").HasColumnName("power").HasDefaultValue(0);
                entity.Property(e => e.Notes).HasColumnType("int(11)").HasColumnName("notes").HasDefaultValue(0);
                entity.Property(e => e.Peak).HasColumnType("int(11)").HasColumnName("peak").HasDefaultValue(0);
                entity.Property(e => e.Tsumami).HasColumnType("int(11)").HasColumnName("tsumami").HasDefaultValue(0);
                entity.Property(e => e.Tricky).HasColumnType("int(11)").HasColumnName("tricky").HasDefaultValue(0);
                entity.Property(e => e.Onehand).HasColumnType("int(11)").HasColumnName("onehand").HasDefaultValue(0);
                entity.Property(e => e.Handtrip).HasColumnType("int(11)").HasColumnName("handtrip").HasDefaultValue(0);
                entity.Property(e => e.OverRadar).HasMaxLength(-1).HasColumnName("over_radar").HasDefaultValueSql("''");

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvVariantPowers)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_variant_profile_to_profile(id)");
            });

            builder.Entity<SvSkill>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_skill", tb => tb.HasComment("Data store(Skill) for Sound Voltex"));

                entity.HasIndex(e => e.Profile, "FK_skill_profile_to_profile(id)");
                entity.HasIndex(e => new { e.Profile, e.Version }, "idx_profile_version");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Profile).HasColumnType("int(11)").HasColumnName("profile");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version").HasDefaultValue(6);
                entity.Property(e => e.Base).HasColumnType("smallint(6)").HasColumnName("base").HasDefaultValue((short)0);
                entity.Property(e => e.Level).HasColumnType("smallint(6)").HasColumnName("level").HasDefaultValue((short)0);
                entity.Property(e => e.Name).HasColumnType("smallint(6)").HasColumnName("name").HasDefaultValue((short)0);
                entity.Property(e => e.Type).HasColumnType("smallint(6)").HasColumnName("type").HasDefaultValue((short)0);

                entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.SvSkills)
                    .HasForeignKey(d => d.Profile)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_skill_profile_to_profile(id)");
            });

            builder.Entity<SvWeeklyMusicScore>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_weekly_music_score", tb => tb.HasComment("Data store(Weekly Music Score) for Sound Voltex"));

                entity.HasIndex(e => new { e.Week, e.Mid, e.Mtype, e.Version }, "idx_week_mid_mtype_version");
                entity.HasIndex(e => new { e.Week, e.Mid, e.Mtype, e.Version, e.Exscore }, "idx_rank_list");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.RefId).HasMaxLength(16).IsFixedLength().HasColumnName("ref_id");
                entity.Property(e => e.Week).HasColumnType("int(11)").HasColumnName("week");
                entity.Property(e => e.Mid).HasColumnType("int(11)").HasColumnName("mid");
                entity.Property(e => e.Mtype).HasColumnType("int(11)").HasColumnName("mtype");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version").HasDefaultValue(6);
                entity.Property(e => e.Exscore).HasColumnType("int(11)").HasColumnName("exscore").HasDefaultValue(0);
                entity.Property(e => e.Name).HasMaxLength(8).HasColumnName("name");
                entity.Property(e => e.PlayCount).HasColumnType("int(11)").HasColumnName("play_count").HasDefaultValue(0);
                entity.Property(e => e.HiscoreCount).HasColumnType("int(11)").HasColumnName("hiscore_count").HasDefaultValue(0);
            });

            builder.Entity<SvPolicyBreak>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_policy_break", tb => tb.HasComment("Data store(Policy Break) for Sound Voltex"));

                entity.HasIndex(e => new { e.RefId, e.Version, e.Id1 }, "idx_refid_version_id");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.RefId).HasMaxLength(16).IsFixedLength().HasColumnName("ref_id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version").HasDefaultValue(2);
                entity.Property(e => e.Id1).HasColumnType("int(11)").HasColumnName("pb_id");
                entity.Property(e => e.Exp).HasColumnType("int(11)").HasColumnName("exp").HasDefaultValue(0);
            });

            builder.Entity<SvCounter>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("sv_counter", tb => tb.HasComment("Data store(Counter) for Sound Voltex"));

                entity.HasIndex(e => e.Key, "idx_key").IsUnique();

                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Key).HasMaxLength(32).HasColumnName("key");
                entity.Property(e => e.Value).HasColumnType("int(11)").HasColumnName("value").HasDefaultValue(0);
            });

            ConfigureStaticData(builder);
        }

        private static void ConfigureStaticData(ModelBuilder builder)
        {
            builder.Entity<StaticData.SvEventData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_event");
                entity.HasIndex(e => new { e.Version, e.SortOrder }, "idx_version_sort");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.SortOrder).HasColumnType("int(11)").HasColumnName("sort_order");
                entity.Property(e => e.EventId).HasColumnType("longtext").HasColumnName("event_id");
                entity.Property(e => e.ToggleKey).HasMaxLength(64).HasColumnName("toggle_key");
            });

            builder.Entity<StaticData.SvCourseData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_course");
                entity.HasIndex(e => new { e.Version, e.SeriesId }, "idx_version_series");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.SeriesId).HasColumnType("int(11)").HasColumnName("series_id");
                entity.Property(e => e.SeriesName).HasMaxLength(128).HasColumnName("series_name");
                entity.Property(e => e.IsNew).HasColumnType("tinyint(1)").HasColumnName("is_new");
                entity.Property(e => e.HasGod).HasColumnType("smallint(6)").HasColumnName("has_god").HasDefaultValue((short)0);
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
                entity.Property(e => e.CoursesJson).HasMaxLength(-1).HasColumnName("courses_json");
            });

            builder.Entity<StaticData.SvValgeneData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_valgene");
                entity.HasIndex(e => new { e.Version, e.ValgeneId }, "idx_version_valgene_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.ValgeneId).HasColumnType("int(11)").HasColumnName("valgene_id");
                entity.Property(e => e.ValgeneName).HasMaxLength(128).HasColumnName("valgene_name");
                entity.Property(e => e.ValgeneNameEnglish).HasMaxLength(128).HasColumnName("valgene_name_english");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
            });

            builder.Entity<StaticData.SvValgeneCatalog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_valgene_catalog");
                entity.HasIndex(e => new { e.Version, e.ValgeneId }, "idx_version_valgene_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.ValgeneId).HasColumnType("int(11)").HasColumnName("valgene_id");
                entity.Property(e => e.ItemType).HasColumnType("int(11)").HasColumnName("item_type");
                entity.Property(e => e.ItemId).HasColumnType("int(11)").HasColumnName("item_id");
                entity.Property(e => e.Rarity).HasColumnType("int(11)").HasColumnName("rarity");
            });

            builder.Entity<StaticData.SvApigeneData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_apigene");
                entity.HasIndex(e => new { e.Version, e.ApigeneId }, "idx_version_apigene_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.ApigeneId).HasColumnType("int(11)").HasColumnName("apigene_id");
                entity.Property(e => e.Name).HasMaxLength(128).HasColumnName("name");
                entity.Property(e => e.NameEnglish).HasMaxLength(128).HasColumnName("name_english");
                entity.Property(e => e.CommonRate).HasColumnType("int(11)").HasColumnName("common_rate");
                entity.Property(e => e.UncommonRate).HasColumnType("int(11)").HasColumnName("uncommon_rate");
                entity.Property(e => e.RareRate).HasColumnType("int(11)").HasColumnName("rare_rate");
                entity.Property(e => e.Price).HasColumnType("int(11)").HasColumnName("price");
                entity.Property(e => e.NoDuplicate).HasColumnType("tinyint(1)").HasColumnName("no_duplicate");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
            });

            builder.Entity<StaticData.SvApigeneCatalog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_apigene_catalog");
                entity.HasIndex(e => new { e.Version, e.ApigeneId }, "idx_version_apigene_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.ApigeneId).HasColumnType("int(11)").HasColumnName("apigene_id");
                entity.Property(e => e.ItemType).HasColumnType("int(11)").HasColumnName("item_type");
                entity.Property(e => e.ItemId).HasColumnType("int(11)").HasColumnName("item_id");
                entity.Property(e => e.Rarity).HasColumnType("int(11)").HasColumnName("rarity");
            });

            builder.Entity<StaticData.SvExtendData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_extend");
                entity.HasIndex(e => new { e.Version, e.ExtendId }, "idx_version_extend_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.ExtendId).HasColumnType("int(10) unsigned").HasColumnName("extend_id");
                entity.Property(e => e.ExtendType).HasColumnType("int(10) unsigned").HasColumnName("extend_type");
                entity.Property(e => e.ParamNum1).HasColumnType("int(11)").HasColumnName("param_num_1");
                entity.Property(e => e.ParamNum2).HasColumnType("int(11)").HasColumnName("param_num_2");
                entity.Property(e => e.ParamNum3).HasColumnType("int(11)").HasColumnName("param_num_3");
                entity.Property(e => e.ParamNum4).HasColumnType("int(11)").HasColumnName("param_num_4");
                entity.Property(e => e.ParamNum5).HasColumnType("int(11)").HasColumnName("param_num_5");
                entity.Property(e => e.ParamStr1).HasColumnType("longtext").HasColumnName("param_str_1");
                entity.Property(e => e.ParamStr2).HasColumnType("longtext").HasColumnName("param_str_2");
                entity.Property(e => e.ParamStr3).HasColumnType("longtext").HasColumnName("param_str_3");
                entity.Property(e => e.ParamStr4).HasColumnType("longtext").HasColumnName("param_str_4");
                entity.Property(e => e.ParamStr5).HasColumnType("longtext").HasColumnName("param_str_5");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
                entity.Property(e => e.StartDate).HasColumnType("int(11)").HasColumnName("start_date");
            });

            builder.Entity<StaticData.SvInformationData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_information");
                entity.HasIndex(e => new { e.Version, e.InfoId }, "idx_version_info_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.InfoId).HasColumnType("int(11)").HasColumnName("info_id");
                entity.Property(e => e.InfoStr).HasColumnType("longtext").HasColumnName("info_str");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
                entity.Property(e => e.StartDate).HasColumnType("int(11)").HasColumnName("start_date");
            });

            builder.Entity<StaticData.SvUnlockEventData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_unlock_event");
                entity.HasIndex(e => new { e.Version, e.EventId }, "idx_version_event_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.EventId).HasMaxLength(64).HasColumnName("event_id");
                entity.Property(e => e.Type).HasMaxLength(32).HasColumnName("type");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
                entity.Property(e => e.StartDate).HasColumnType("int(11)").HasColumnName("start_date");
                entity.Property(e => e.DataJson).HasMaxLength(-1).HasColumnName("data_json");
                entity.Property(e => e.ItemsJson).HasMaxLength(-1).HasColumnName("items_json");
                entity.Property(e => e.TogglesJson).HasMaxLength(-1).HasColumnName("toggles_json");
                entity.Property(e => e.SettingsJson).HasMaxLength(-1).HasColumnName("settings_json");
            });

            builder.Entity<StaticData.SvArenaStationItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_arena_station");
                entity.HasIndex(e => new { e.Version, e.SetName }, "idx_version_set_name");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.SetName).HasMaxLength(64).HasColumnName("set_name");
                entity.Property(e => e.MinVersion).HasColumnType("int(11)").HasColumnName("min_version");
                entity.Property(e => e.ItemsJson).HasMaxLength(-1).HasColumnName("items_json");
            });

            builder.Entity<StaticData.SvCurrentArena>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_current_arena");
                entity.HasIndex(e => e.Version, "idx_version");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.Season).HasColumnType("int(11)").HasColumnName("season");
                entity.Property(e => e.Rule).HasColumnType("smallint(6)").HasColumnName("rule");
                entity.Property(e => e.RankMatchTarget).HasColumnType("smallint(6)").HasColumnName("rank_match_target");
                entity.Property(e => e.TimeStart).HasColumnType("bigint(20)").HasColumnName("time_start");
                entity.Property(e => e.TimeEnd).HasColumnType("bigint(20)").HasColumnName("time_end");
                entity.Property(e => e.ShopStart).HasColumnType("bigint(20)").HasColumnName("shop_start");
                entity.Property(e => e.ShopEnd).HasColumnType("bigint(20)").HasColumnName("shop_end");
            });

            builder.Entity<StaticData.SvMusicOverride>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_music_override");
                entity.HasIndex(e => new { e.Version, e.MusicId }, "idx_version_music_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
                entity.Property(e => e.StartDate).HasColumnType("int(11)").HasColumnName("start_date");
                entity.Property(e => e.InfoJson).HasMaxLength(-1).HasColumnName("info_json");
                entity.Property(e => e.ChartsJson).HasMaxLength(-1).HasColumnName("charts_json");
            });

            builder.Entity<StaticData.SvLicensedSong>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_licensed_song");
                entity.HasIndex(e => new { e.Version, e.MusicId }, "idx_version_music_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
            });

            builder.Entity<StaticData.SvEgSongLocked>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_eg_song_locked");
                entity.HasIndex(e => new { e.Version, e.Category }, "idx_version_category");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.Category).HasMaxLength(64).HasColumnName("category");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
            });

            builder.Entity<StaticData.SvMegamixData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_megamix");
                entity.HasIndex(e => new { e.Version, e.MegamixNo }, "idx_version_megamix_no");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.MegamixNo).HasColumnType("int(11)").HasColumnName("megamix_no");
                entity.Property(e => e.SongIds).HasColumnType("longtext").HasColumnName("song_ids");
            });

            builder.Entity<StaticData.SvAprilFoolsSong>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_april_fools_song");
                entity.HasIndex(e => new { e.Version, e.MusicId }, "idx_version_music_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
            });

            builder.Entity<StaticData.SvValkyrieSong>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_valkyrie_song");
                entity.HasIndex(e => new { e.Version, e.MusicId }, "idx_version_music_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
            });

            builder.Entity<StaticData.SvPolicyBreakData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_policy_break");
                entity.HasIndex(e => new { e.Version, e.Pbid }, "idx_version_pbid");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.Version).HasColumnType("int(11)").HasColumnName("version");
                entity.Property(e => e.Pbid).HasColumnType("int(11)").HasColumnName("pb_id");
                entity.Property(e => e.RwrdType).HasColumnType("int(11)").HasColumnName("rwrd_type");
                entity.Property(e => e.RwrdId).HasColumnType("int(11)").HasColumnName("rwrd_id");
                entity.Property(e => e.RwrdParam).HasColumnType("int(11)").HasColumnName("rwrd_param");
                entity.Property(e => e.StartDate).HasColumnType("bigint(20)").HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnType("bigint(20)").HasColumnName("end_date");
            });

            builder.Entity<StaticData.SvWeeklyMusic>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_weekly_music");
                entity.HasIndex(e => e.WeekId, "idx_week_id");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.WeekId).HasColumnType("int(11)").HasColumnName("week_id");
                entity.Property(e => e.MusicId).HasColumnType("int(11)").HasColumnName("music_id");
                entity.Property(e => e.Start).HasColumnType("bigint(20)").HasColumnName("start");
                entity.Property(e => e.End).HasColumnType("bigint(20)").HasColumnName("end");
            });

            builder.Entity<StaticData.SvHaveNote>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("sv_static_have_note");
                entity.HasIndex(e => new { e.NoteId, e.Param }, "idx_note_param");
                entity.Property(e => e.Id).HasColumnType("int(11)").HasColumnName("id");
                entity.Property(e => e.NoteId).HasColumnType("int(11)").HasColumnName("note_id");
                entity.Property(e => e.Param).HasColumnType("int(11)").HasColumnName("param");
            });
        }
    }
}
