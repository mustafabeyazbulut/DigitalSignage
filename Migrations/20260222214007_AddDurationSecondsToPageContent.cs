using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationSecondsToPageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "PageContents",
                type: "int",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "PageContents");
        }
    }
}
