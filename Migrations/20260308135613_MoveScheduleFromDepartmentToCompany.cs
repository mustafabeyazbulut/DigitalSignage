using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class MoveScheduleFromDepartmentToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Önce mevcut DepartmentID değerlerini doğru CompanyID'ye dönüştür
            migrationBuilder.Sql(@"
                UPDATE s SET s.DepartmentID = d.CompanyID
                FROM Schedules s
                INNER JOIN Departments d ON s.DepartmentID = d.DepartmentID
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Departments_DepartmentID",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "DepartmentID",
                table: "Schedules",
                newName: "CompanyID");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DepartmentID_IsActive",
                table: "Schedules",
                newName: "IX_Schedules_CompanyID_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DepartmentID",
                table: "Schedules",
                newName: "IX_Schedules_CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Companies_CompanyID",
                table: "Schedules",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Companies_CompanyID",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "CompanyID",
                table: "Schedules",
                newName: "DepartmentID");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_CompanyID_IsActive",
                table: "Schedules",
                newName: "IX_Schedules_DepartmentID_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_CompanyID",
                table: "Schedules",
                newName: "IX_Schedules_DepartmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Departments_DepartmentID",
                table: "Schedules",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
