using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class KfcSv3Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "rwrd_music_id",
                table: "sv_static_policy_break",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rwrd_point",
                table: "sv_static_policy_break",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "target_id",
                table: "sv_static_policy_break",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "title_e",
                table: "sv_static_policy_break",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "title_j",
                table: "sv_static_policy_break",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sv3_story",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ref_id = table.Column<string>(type: "char(16)", fixedLength: true, maxLength: 16, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<int>(type: "int(11)", nullable: false),
                    story_id = table.Column<int>(type: "int(11)", nullable: false),
                    progress_id = table.Column<int>(type: "int(11)", nullable: false),
                    progress_param = table.Column<int>(type: "int(11)", nullable: false),
                    clear_cnt = table.Column<int>(type: "int(11)", nullable: false),
                    route_flg = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "GRAVITY WARS (sv3) story progression data")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "idx_refid_version_storyid",
                table: "sv3_story",
                columns: new[] { "ref_id", "version", "story_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sv3_story");

            migrationBuilder.DropColumn(
                name: "rwrd_music_id",
                table: "sv_static_policy_break");

            migrationBuilder.DropColumn(
                name: "rwrd_point",
                table: "sv_static_policy_break");

            migrationBuilder.DropColumn(
                name: "target_id",
                table: "sv_static_policy_break");

            migrationBuilder.DropColumn(
                name: "title_e",
                table: "sv_static_policy_break");

            migrationBuilder.DropColumn(
                name: "title_j",
                table: "sv_static_policy_break");
        }
    }
}
