using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class kfcplugininital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sv_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    @event = table.Column<string>(name: "event", type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Events) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_music",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title_yomigana = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    artist = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    artist_yomigana = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Music) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_profile",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    card = table.Column<int>(type: "int(11)", nullable: false),
                    name = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false, defaultValueSql: "'VOLTEX'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "char(10)", fixedLength: true, maxLength: 10, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    appeal_id = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false),
                    last_music_id = table.Column<int>(type: "int(11)", nullable: false),
                    last_music_type = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    sort_type = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    headphone = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    blaster_energy = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    blaster_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    extrack_energy = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false),
                    hispeed = table.Column<int>(type: "int(11)", nullable: false),
                    lanespeed = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    gauge_option = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    ars_option = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    notes_option = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    early_late_disp = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    draw_adjust = table.Column<int>(type: "int(11)", nullable: false),
                    eff_c_left = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    eff_c_right = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValueSql: "'1'"),
                    kac_id = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false, defaultValueSql: "'VOLTEX'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    skill_level = table.Column<short>(type: "smallint(6)", nullable: false),
                    skill_base_id = table.Column<short>(type: "smallint(6)", nullable: false),
                    skill_name_id = table.Column<short>(type: "smallint(6)", nullable: false),
                    blaster_pass_enable = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    blaster_pass_limit_date = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false),
                    pcb = table.Column<int>(type: "int(11)", nullable: false, comment: "equals with block_no"),
                    play_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    day_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    today_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    play_chain = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    max_play_chain = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    week_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    week_play_count = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    week_chain = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    max_week_chain = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    bgm = table.Column<int>(type: "int(11)", nullable: false),
                    sub_bg = table.Column<int>(type: "int(11)", nullable: false),
                    nemsys = table.Column<int>(type: "int(11)", nullable: false),
                    stampA = table.Column<int>(type: "int(11)", nullable: false),
                    stampB = table.Column<int>(type: "int(11)", nullable: false),
                    stampC = table.Column<int>(type: "int(11)", nullable: false),
                    stampD = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "Data store(Profile) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_course_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    series_id = table.Column<int>(type: "int(11)", nullable: false),
                    course_id = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    score = table.Column<int>(type: "int(11)", nullable: false),
                    clear = table.Column<int>(type: "int(11)", nullable: false),
                    grade = table.Column<int>(type: "int(11)", nullable: false),
                    rate = table.Column<int>(type: "int(11)", nullable: false),
                    count = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Course Records) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false),
                    item_id = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    param = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_profile_to_card(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Data store(Items) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_matchmakers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    timestamp = table.Column<long>(type: "bigint(20)", nullable: false),
                    c_version = table.Column<int>(type: "int(11)", nullable: false),
                    player_num = table.Column<int>(type: "int(11)", nullable: false),
                    player_remaining = table.Column<int>(type: "int(11)", nullable: false),
                    filter = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false),
                    seconds = table.Column<int>(type: "int(11)", nullable: false),
                    port = table.Column<int>(type: "int(11)", nullable: false),
                    global_ip = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    local_ip = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claim = table.Column<int>(type: "int(11)", nullable: false),
                    entry_id = table.Column<int>(type: "int(11)", nullable: false),
                    SvProfileId = table.Column<int>(type: "int(11)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_sv_matchmakers_sv_profile_SvProfileId",
                        column: x => x.SvProfileId,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Matchmaker) for Sound Voltex global matching")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_params",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false),
                    param_id = table.Column<int>(type: "int(11)", nullable: false),
                    param = table.Column<string>(type: "longtext", maxLength: -1, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    param_count = table.Column<uint>(type: "int(11) unsigned", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_param_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Params) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_rivals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ref_id = table.Column<string>(type: "char(16)", fixedLength: true, maxLength: 16, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rival_ref_id = table.Column<string>(type: "char(16)", fixedLength: true, maxLength: 16, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sdvx_id = table.Column<int>(type: "int(11)", nullable: false),
                    name = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mutual = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6),
                    ProfileNavigationId = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_sv_rivals_sv_profile_ProfileNavigationId",
                        column: x => x.ProfileNavigationId,
                        principalTable: "sv_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Data store(Rivals) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_scores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    music_id = table.Column<int>(type: "int(11)", nullable: false),
                    type = table.Column<int>(type: "int(11)", nullable: false),
                    score = table.Column<int>(type: "int(11)", nullable: false),
                    exscore = table.Column<int>(type: "int(11)", nullable: false),
                    clear = table.Column<int>(type: "int(11)", nullable: false),
                    grade = table.Column<int>(type: "int(11)", nullable: false),
                    buttonRate = table.Column<int>(type: "int(11)", nullable: false),
                    longRate = table.Column<int>(type: "int(11)", nullable: false),
                    volRate = table.Column<int>(type: "int(11)", nullable: false),
                    version = table.Column<int>(type: "int(11)", nullable: false, defaultValue: 6)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_musicid_to_music(id)",
                        column: x => x.music_id,
                        principalTable: "sv_music",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Scores) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "sv_valgene_tickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    profile = table.Column<int>(type: "int(11)", nullable: false),
                    ticket_num = table.Column<int>(type: "int(11)", nullable: false),
                    limit_date = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_valgene_profile_to_profile(id)",
                        column: x => x.profile,
                        principalTable: "sv_profile",
                        principalColumn: "id");
                },
                comment: "Data store(Valgene Tickets) for Sound Voltex")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "FK_course_profile_to_profile(id)",
                table: "sv_course_records",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "FK_profile_to_card(id)",
                table: "sv_items",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "idx_matchmaker_search",
                table: "sv_matchmakers",
                columns: new[] { "version", "c_version", "filter", "claim", "entry_id" });

            migrationBuilder.CreateIndex(
                name: "idx_version_timestamp",
                table: "sv_matchmakers",
                columns: new[] { "version", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_sv_matchmakers_SvProfileId",
                table: "sv_matchmakers",
                column: "SvProfileId");

            migrationBuilder.CreateIndex(
                name: "FK_param_profile_to_profile(id)",
                table: "sv_params",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "card",
                table: "sv_profile",
                column: "card",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refid_version",
                table: "sv_rivals",
                columns: new[] { "ref_id", "version" });

            migrationBuilder.CreateIndex(
                name: "IX_sv_rivals_ProfileNavigationId",
                table: "sv_rivals",
                column: "ProfileNavigationId");

            migrationBuilder.CreateIndex(
                name: "FK_musicid_to_music(id)",
                table: "sv_scores",
                column: "music_id");

            migrationBuilder.CreateIndex(
                name: "FK_profile_to_profile(id)",
                table: "sv_scores",
                column: "profile");

            migrationBuilder.CreateIndex(
                name: "FK_valgene_profile_to_profile(id)",
                table: "sv_valgene_tickets",
                column: "profile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sv_course_records");

            migrationBuilder.DropTable(
                name: "sv_events");

            migrationBuilder.DropTable(
                name: "sv_items");

            migrationBuilder.DropTable(
                name: "sv_matchmakers");

            migrationBuilder.DropTable(
                name: "sv_params");

            migrationBuilder.DropTable(
                name: "sv_rivals");

            migrationBuilder.DropTable(
                name: "sv_scores");

            migrationBuilder.DropTable(
                name: "sv_valgene_tickets");

            migrationBuilder.DropTable(
                name: "sv_music");

            migrationBuilder.DropTable(
                name: "sv_profile");
        }
    }
}
