using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemContentWidgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DepartmentID",
                table: "Contents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemContent",
                table: "Contents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Contents_IsSystemContent",
                table: "Contents",
                column: "IsSystemContent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contents_IsSystemContent",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "IsSystemContent",
                table: "Contents");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentID",
                table: "Contents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
