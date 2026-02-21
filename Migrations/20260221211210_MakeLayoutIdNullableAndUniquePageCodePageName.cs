using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class MakeLayoutIdNullableAndUniquePageCodePageName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LayoutID",
                table: "Pages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_PageCode",
                table: "Pages",
                column: "PageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_PageName",
                table: "Pages",
                column: "PageName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pages_PageCode",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_PageName",
                table: "Pages");

            migrationBuilder.AlterColumn<int>(
                name: "LayoutID",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
