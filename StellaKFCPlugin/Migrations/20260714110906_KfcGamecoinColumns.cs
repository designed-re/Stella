using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class KfcGamecoinColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "blocks",
                table: "sv_profile",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 10000u);

            migrationBuilder.AddColumn<uint>(
                name: "packets",
                table: "sv_profile",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 10000u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blocks",
                table: "sv_profile");

            migrationBuilder.DropColumn(
                name: "packets",
                table: "sv_profile");
        }
    }
}
