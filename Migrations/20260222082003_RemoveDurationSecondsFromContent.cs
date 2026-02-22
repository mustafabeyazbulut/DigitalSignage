using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDurationSecondsFromContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Contents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Contents",
                type: "int",
                nullable: true);
        }
    }
}
