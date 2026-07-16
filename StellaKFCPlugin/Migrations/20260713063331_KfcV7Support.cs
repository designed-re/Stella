using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class KfcV7Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dbver",
                table: "sv_scores",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "play_count",
                table: "sv_scores",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "volforce",
                table: "sv_scores",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "akaname",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bpl_support",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "creator_item",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "datecode",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "dbver",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "plugin_ver",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "exscore",
                table: "sv_course_records",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "kac_id",
                table: "sv_course_records",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValueSql: "''",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "skill_type",
                table: "sv_course_records",
                type: "smallint(6)",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "sv_arena",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    season = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    rank_point = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    shop_point = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    ultimate_rate = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    ultimate_rank_num = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    megamix_rate = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    rank_count = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    ultimate_count = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    live_energy = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_arena_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Arena) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_counter",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    key = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    value = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Counter) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_policy_break",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ref_id = table.Column<string>(type: "char(16)", fixedLength: true, maxLength: 16, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 2),
                    pb_id = table.Column<int>(type: "int(11)", nullable: false),
                    exp = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Policy Break) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_skill",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    @base = table.Column<short>(name: "base", type: "smallint(6)", nullable: false, defaultValue: (short)0),
                    level = table.Column<short>(type: "smallint(6)", nullable: false, defaultValue: (short)0),
                    name = table.Column<short>(type: "smallint(6)", nullable: false, defaultValue: (short)0),
                    type = table.Column<short>(type: "smallint(6)", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_skill_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Skill) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_apigene",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    apigene_id = table.Column<int>(type: "int(11)", nullable: false),
                    name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_english = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    common_rate = table.Column<int>(type: "int(11)", nullable: false),
                    uncommon_rate = table.Column<int>(type: "int(11)", nullable: false),
                    rare_rate = table.Column<int>(type: "int(11)", nullable: false),
                    price = table.Column<int>(type: "int(11)", nullable: false),
                    no_duplicate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    min_version = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_apigene_catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    apigene_id = table.Column<int>(type: "int(11)", nullable: false),
                    item_type = table.Column<int>(type: "int(11)", nullable: false),
                    item_id = table.Column<int>(type: "int(11)", nullable: false),
                    rarity = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_april_fools_song",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_arena_station",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    set_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_version = table.Column<int>(type: "int(11)", nullable: false),
                    items_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_course",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    series_id = table.Column<int>(type: "int(11)", nullable: false),
                    series_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_new = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_god = table.Column<short>(type: "smallint(6)", nullable: false, defaultValue: (short)0),
                    min_version = table.Column<int>(type: "int(11)", nullable: false),
                    courses_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_current_arena",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    season = table.Column<int>(type: "int(11)", nullable: false),
                    rule = table.Column<short>(type: "smallint(6)", nullable: false),
                    rank_match_target = table.Column<short>(type: "smallint(6)", nullable: false),
                    time_start = table.Column<long>(type: "bigint(20)", nullable: false),
                    time_end = table.Column<long>(type: "bigint(20)", nullable: false),
                    shop_start = table.Column<long>(type: "bigint(20)", nullable: false),
                    shop_end = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_eg_song_locked",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    category = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    music_id = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    sort_order = table.Column<int>(type: "int(11)", nullable: false),
                    event_id = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    toggle_key = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_extend",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    extend_id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    extend_type = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    param_num_1 = table.Column<int>(type: "int(11)", nullable: false),
                    param_num_2 = table.Column<int>(type: "int(11)", nullable: false),
                    param_num_3 = table.Column<int>(type: "int(11)", nullable: false),
                    param_num_4 = table.Column<int>(type: "int(11)", nullable: false),
                    param_num_5 = table.Column<int>(type: "int(11)", nullable: false),
                    param_str_1 = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    param_str_2 = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    param_str_3 = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    param_str_4 = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    param_str_5 = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_version = table.Column<int>(type: "int(11)", nullable: false),
                    start_date = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_have_note",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    note_id = table.Column<int>(type: "int(11)", nullable: false),
                    param = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_information",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    info_id = table.Column<int>(type: "int(11)", nullable: false),
                    info_str = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_version = table.Column<int>(type: "int(11)", nullable: false),
                    start_date = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_licensed_song",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_megamix",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    megamix_no = table.Column<int>(type: "int(11)", nullable: false),
                    song_ids = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_music_override",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false),
                    start_date = table.Column<int>(type: "int(11)", nullable: false),
                    info_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    charts_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_policy_break",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    pb_id = table.Column<int>(type: "int(11)", nullable: false),
                    rwrd_type = table.Column<int>(type: "int(11)", nullable: false),
                    rwrd_id = table.Column<int>(type: "int(11)", nullable: false),
                    rwrd_param = table.Column<int>(type: "int(11)", nullable: false),
                    start_date = table.Column<long>(type: "bigint(20)", nullable: false),
                    end_date = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_unlock_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    event_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_version = table.Column<int>(type: "int(11)", nullable: false),
                    start_date = table.Column<int>(type: "int(11)", nullable: false),
                    data_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    items_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    toggles_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    settings_json = table.Column<string>(type: "longtext", maxLength: -1, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_valgene",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    valgene_id = table.Column<int>(type: "int(11)", nullable: false),
                    valgene_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valgene_name_english = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_version = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_valgene_catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    valgene_id = table.Column<int>(type: "int(11)", nullable: false),
                    item_type = table.Column<int>(type: "int(11)", nullable: false),
                    item_id = table.Column<int>(type: "int(11)", nullable: false),
                    rarity = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_valkyrie_song",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_static_weekly_music",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    week_id = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false),
                    start = table.Column<long>(type: "bigint(20)", nullable: false),
                    end = table.Column<long>(type: "bigint(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_variant_power",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    power = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    notes = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    peak = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    tsumami = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    tricky = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    onehand = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    handtrip = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    over_radar = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, defaultValueSql: "''", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_variant_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Variant Power) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_weekly_music_score",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ref_id = table.Column<string>(type: "char(16)", fixedLength: true, maxLength: 16, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    week = table.Column<int>(type: "int(11)", nullable: false),
                    mid = table.Column<int>(type: "int(11)", nullable: false),
                    mtype = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    exscore = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    play_count = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0),
                    hiscore_count = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Weekly Music Score) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "FK_arena_profile_to_profile(id)",
                table: "sv_arena",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "idx_profile_season_version",
                table: "sv_arena",
                columns: new[] { "profile", "season", "version" });

            migrationBuilder.CreateIndex(
                name: "idx_key",
                table: "sv_counter",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refid_version_id",
                table: "sv_policy_break",
                columns: new[] { "ref_id", "version", "pb_id" });

            migrationBuilder.CreateIndex(
                name: "FK_skill_profile_to_profile(id)",
                table: "sv_skill",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "idx_profile_version",
                table: "sv_skill",
                columns: new[] { "profile", "version" });

            migrationBuilder.CreateIndex(
                name: "idx_version_apigene_id1",
                table: "sv_static_apigene",
                columns: new[] { "version", "apigene_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_apigene_id",
                table: "sv_static_apigene_catalog",
                columns: new[] { "version", "apigene_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_music_id",
                table: "sv_static_april_fools_song",
                columns: new[] { "version", "music_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_set_name",
                table: "sv_static_arena_station",
                columns: new[] { "version", "set_name" });

            migrationBuilder.CreateIndex(
                name: "idx_version_series",
                table: "sv_static_course",
                columns: new[] { "version", "series_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version",
                table: "sv_static_current_arena",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "idx_version_category",
                table: "sv_static_eg_song_locked",
                columns: new[] { "version", "category" });

            migrationBuilder.CreateIndex(
                name: "idx_version_sort",
                table: "sv_static_event",
                columns: new[] { "version", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "idx_version_extend_id",
                table: "sv_static_extend",
                columns: new[] { "version", "extend_id" });

            migrationBuilder.CreateIndex(
                name: "idx_note_param",
                table: "sv_static_have_note",
                columns: new[] { "note_id", "param" });

            migrationBuilder.CreateIndex(
                name: "idx_version_info_id",
                table: "sv_static_information",
                columns: new[] { "version", "info_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_music_id1",
                table: "sv_static_licensed_song",
                columns: new[] { "version", "music_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_megamix_no",
                table: "sv_static_megamix",
                columns: new[] { "version", "megamix_no" });

            migrationBuilder.CreateIndex(
                name: "idx_version_music_id2",
                table: "sv_static_music_override",
                columns: new[] { "version", "music_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_pbid",
                table: "sv_static_policy_break",
                columns: new[] { "version", "pb_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_event_id",
                table: "sv_static_unlock_event",
                columns: new[] { "version", "event_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_valgene_id1",
                table: "sv_static_valgene",
                columns: new[] { "version", "valgene_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_valgene_id",
                table: "sv_static_valgene_catalog",
                columns: new[] { "version", "valgene_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_music_id3",
                table: "sv_static_valkyrie_song",
                columns: new[] { "version", "music_id" });

            migrationBuilder.CreateIndex(
                name: "idx_week_id",
                table: "sv_static_weekly_music",
                column: "week_id");

            migrationBuilder.CreateIndex(
                name: "FK_variant_profile_to_profile(id)",
                table: "sv_variant_power",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "idx_profile_version1",
                table: "sv_variant_power",
                columns: new[] { "profile", "version" });

            migrationBuilder.CreateIndex(
                name: "idx_rank_list",
                table: "sv_weekly_music_score",
                columns: new[] { "week", "mid", "mtype", "version", "exscore" });

            migrationBuilder.CreateIndex(
                name: "idx_week_mid_mtype_version",
                table: "sv_weekly_music_score",
                columns: new[] { "week", "mid", "mtype", "version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sv_arena");

            migrationBuilder.DropTable(
                name: "sv_counter");

            migrationBuilder.DropTable(
                name: "sv_policy_break");

            migrationBuilder.DropTable(
                name: "sv_skill");

            migrationBuilder.DropTable(
                name: "sv_static_apigene");

            migrationBuilder.DropTable(
                name: "sv_static_apigene_catalog");

            migrationBuilder.DropTable(
                name: "sv_static_april_fools_song");

            migrationBuilder.DropTable(
                name: "sv_static_arena_station");

            migrationBuilder.DropTable(
                name: "sv_static_course");

            migrationBuilder.DropTable(
                name: "sv_static_current_arena");

            migrationBuilder.DropTable(
                name: "sv_static_eg_song_locked");

            migrationBuilder.DropTable(
                name: "sv_static_event");

            migrationBuilder.DropTable(
                name: "sv_static_extend");

            migrationBuilder.DropTable(
                name: "sv_static_have_note");

            migrationBuilder.DropTable(
                name: "sv_static_information");

            migrationBuilder.DropTable(
                name: "sv_static_licensed_song");

            migrationBuilder.DropTable(
                name: "sv_static_megamix");

            migrationBuilder.DropTable(
                name: "sv_static_music_override");

            migrationBuilder.DropTable(
                name: "sv_static_policy_break");

            migrationBuilder.DropTable(
                name: "sv_static_unlock_event");

            migrationBuilder.DropTable(
                name: "sv_static_valgene");

            migrationBuilder.DropTable(
                name: "sv_static_valgene_catalog");

            migrationBuilder.DropTable(
                name: "sv_static_valkyrie_song");

            migrationBuilder.DropTable(
                name: "sv_static_weekly_music");

            migrationBuilder.DropTable(
                name: "sv_variant_power");

            migrationBuilder.DropTable(
                name: "sv_weekly_music_score");

            migrationBuilder.DropColumn(
                name: "dbver",
                table: "sv_scores");

            migrationBuilder.DropColumn(
                name: "play_count",
                table: "sv_scores");

            migrationBuilder.DropColumn(
                name: "volforce",
                table: "sv_scores");

            migrationBuilder.DropColumn(
                name: "akaname",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "bpl_support",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "creator_item",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "datecode",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "dbver",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "plugin_ver",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "exscore",
                table: "sv_course_records");

            migrationBuilder.DropColumn(
                name: "kac_id",
                table: "sv_course_records");

            migrationBuilder.DropColumn(
                name: "skill_type",
                table: "sv_course_records");
        }
    }
}
