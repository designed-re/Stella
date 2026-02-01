using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class kfcpluginupdatesvprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "card",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "card",
                table: "sv_profile");

            migrationBuilder.AddColumn<string>(
                name: "refid",
                table: "sv_profile",
                type: "char(16)",
                fixedLength: true,
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "refid",
                table: "sv_profile",
                column: "refid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "refid",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "refid",
                table: "sv_profile");

            migrationBuilder.AddColumn<int>(
                name: "card",
                table: "sv_profile",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "card",
                table: "sv_profile",
                column: "card",
                unique: true);
        }
    }
}
