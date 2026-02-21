using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DepartmentID_IsActive",
                table: "Schedules",
                columns: new[] { "DepartmentID", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_DepartmentID_IsActive",
                table: "Pages",
                columns: new[] { "DepartmentID", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Contents_DepartmentID_CreatedDate",
                table: "Contents",
                columns: new[] { "DepartmentID", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_DepartmentID_IsActive",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Pages_DepartmentID_IsActive",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Contents_DepartmentID_CreatedDate",
                table: "Contents");
        }
    }
}
