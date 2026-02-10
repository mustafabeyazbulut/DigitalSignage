using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailDomain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondaryColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsOffice365User = table.Column<bool>(type: "bit", nullable: false),
                    AzureADObjectId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "CompanyConfigurations",
                columns: table => new
                {
                    ConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    EmailSmtpServer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailSmtpPort = table.Column<int>(type: "int", nullable: false),
                    EmailFrom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailUsername = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnScheduleChange = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnContentChange = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnError = table.Column<bool>(type: "bit", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DefaultGridColumnsX = table.Column<int>(type: "int", nullable: false),
                    DefaultGridRowsY = table.Column<int>(type: "int", nullable: false),
                    DefaultSectionPadding = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultSectionBorder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxSchedulesPerPage = table.Column<int>(type: "int", nullable: false),
                    DefaultScheduleDuration = table.Column<int>(type: "int", nullable: false),
                    AllowRecurringSchedules = table.Column<bool>(type: "bit", nullable: false),
                    ScreenRefreshInterval = table.Column<int>(type: "int", nullable: false),
                    EnableAutoRotation = table.Column<bool>(type: "bit", nullable: false),
                    CustomCSS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableAnalytics = table.Column<bool>(type: "bit", nullable: false),
                    EnableAdvancedScheduling = table.Column<bool>(type: "bit", nullable: false),
                    EnableMediaUpload = table.Column<bool>(type: "bit", nullable: false),
                    MaxMediaSizeGB = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyConfigurations", x => x.ConfigurationID);
                    table.ForeignKey(
                        name: "FK_CompanyConfigurations_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    LayoutID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    LayoutName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LayoutCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GridColumnsX = table.Column<int>(type: "int", nullable: false),
                    GridRowsY = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.LayoutID);
                    table.ForeignKey(
                        name: "FK_Layouts_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID");
                });

            migrationBuilder.CreateTable(
                name: "UserCompanyRoles",
                columns: table => new
                {
                    UserCompanyRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompanyRoles", x => x.UserCompanyRoleID);
                    table.ForeignKey(
                        name: "FK_UserCompanyRoles_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompanyRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    ContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContentTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.ContentID);
                    table.ForeignKey(
                        name: "FK_Contents_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_Schedules_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LayoutSections",
                columns: table => new
                {
                    LayoutSectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LayoutID = table.Column<int>(type: "int", nullable: false),
                    SectionPosition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Height = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutSections", x => x.LayoutSectionID);
                    table.ForeignKey(
                        name: "FK_LayoutSections_Layouts_LayoutID",
                        column: x => x.LayoutID,
                        principalTable: "Layouts",
                        principalColumn: "LayoutID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    PageName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PageCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LayoutID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_Pages_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pages_Layouts_LayoutID",
                        column: x => x.LayoutID,
                        principalTable: "Layouts",
                        principalColumn: "LayoutID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageContents",
                columns: table => new
                {
                    PageContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageID = table.Column<int>(type: "int", nullable: false),
                    ContentID = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DisplaySection = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContents", x => x.PageContentID);
                    table.ForeignKey(
                        name: "FK_PageContents_Contents_ContentID",
                        column: x => x.ContentID,
                        principalTable: "Contents",
                        principalColumn: "ContentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageContents_Pages_PageID",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageSections",
                columns: table => new
                {
                    PageSectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageID = table.Column<int>(type: "int", nullable: false),
                    LayoutSectionID = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSections", x => x.PageSectionID);
                    table.ForeignKey(
                        name: "FK_PageSections_LayoutSections_LayoutSectionID",
                        column: x => x.LayoutSectionID,
                        principalTable: "LayoutSections",
                        principalColumn: "LayoutSectionID");
                    table.ForeignKey(
                        name: "FK_PageSections_Pages_PageID",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePages",
                columns: table => new
                {
                    SchedulePageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    PageID = table.Column<int>(type: "int", nullable: false),
                    DisplayDuration = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePages", x => x.SchedulePageID);
                    table.ForeignKey(
                        name: "FK_SchedulePages_Pages_PageID",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchedulePages_Schedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "Schedules",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyCode",
                table: "Companies",
                column: "CompanyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyConfigurations_CompanyID",
                table: "CompanyConfigurations",
                column: "CompanyID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contents_DepartmentID",
                table: "Contents",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyID",
                table: "Departments",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_CompanyID",
                table: "Layouts",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutSections_LayoutID",
                table: "LayoutSections",
                column: "LayoutID");

            migrationBuilder.CreateIndex(
                name: "IX_PageContents_ContentID",
                table: "PageContents",
                column: "ContentID");

            migrationBuilder.CreateIndex(
                name: "IX_PageContents_PageID",
                table: "PageContents",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_DepartmentID",
                table: "Pages",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_LayoutID",
                table: "Pages",
                column: "LayoutID");

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_LayoutSectionID",
                table: "PageSections",
                column: "LayoutSectionID");

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_PageID",
                table: "PageSections",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePages_PageID",
                table: "SchedulePages",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePages_ScheduleID",
                table: "SchedulePages",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DepartmentID",
                table: "Schedules",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyRoles_CompanyID",
                table: "UserCompanyRoles",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyRoles_UserID",
                table: "UserCompanyRoles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyConfigurations");

            migrationBuilder.DropTable(
                name: "PageContents");

            migrationBuilder.DropTable(
                name: "PageSections");

            migrationBuilder.DropTable(
                name: "SchedulePages");

            migrationBuilder.DropTable(
                name: "UserCompanyRoles");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "LayoutSections");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Layouts");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
