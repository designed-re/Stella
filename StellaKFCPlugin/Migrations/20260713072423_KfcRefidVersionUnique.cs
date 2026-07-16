using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class KfcRefidVersionUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "refid",
                table: "sv_profile");

            migrationBuilder.RenameIndex(
                name: "idx_refid_version",
                table: "sv_rivals",
                newName: "idx_refid_version1");

            migrationBuilder.CreateIndex(
                name: "idx_refid_version",
                table: "sv_profile",
                columns: new[] { "refid", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "refid",
                table: "sv_profile",
                column: "refid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_refid_version",
                table: "sv_profile");

            migrationBuilder.DropIndex(
                name: "refid",
                table: "sv_profile");

            migrationBuilder.RenameIndex(
                name: "idx_refid_version1",
                table: "sv_rivals",
                newName: "idx_refid_version");

            migrationBuilder.CreateIndex(
                name: "refid",
                table: "sv_profile",
                column: "refid",
                unique: true);
        }
    }
}
