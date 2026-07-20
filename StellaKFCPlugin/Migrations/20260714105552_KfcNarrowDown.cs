using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellaKFCPlugin.Migrations
{
    /// <inheritdoc />
    public partial class KfcNarrowDown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "NarrowDown",
                table: "sv_profile",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NarrowDown",
                table: "sv_profile");
        }
    }
}
