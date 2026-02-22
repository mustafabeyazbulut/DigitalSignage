using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class RenameLogoUrlToLogoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "Companies",
                newName: "LogoPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogoPath",
                table: "Companies",
                newName: "LogoUrl");
        }
    }
}
