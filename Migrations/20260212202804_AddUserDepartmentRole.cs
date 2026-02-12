using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDepartmentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserCompanyRoles_UserID",
                table: "UserCompanyRoles");

            migrationBuilder.CreateTable(
                name: "UserDepartmentRoles",
                columns: table => new
                {
                    UserDepartmentRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartmentRoles", x => x.UserDepartmentRoleID);
                    table.ForeignKey(
                        name: "FK_UserDepartmentRoles_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartmentRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyRoles_UserID_CompanyID",
                table: "UserCompanyRoles",
                columns: new[] { "UserID", "CompanyID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartmentRoles_DepartmentID",
                table: "UserDepartmentRoles",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartmentRoles_UserID_DepartmentID",
                table: "UserDepartmentRoles",
                columns: new[] { "UserID", "DepartmentID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDepartmentRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserCompanyRoles_UserID_CompanyID",
                table: "UserCompanyRoles");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyRoles_UserID",
                table: "UserCompanyRoles",
                column: "UserID");
        }
    }
}
