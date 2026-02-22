using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Migrations
{
    /// <inheritdoc />
    public partial class DynamicLayoutDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Yeni LayoutDefinition sütununu varsayılan değerle ekle
            migrationBuilder.AddColumn<string>(
                name: "LayoutDefinition",
                table: "Layouts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{\"rows\":[{\"height\":100,\"columns\":[{\"width\":100}]}]}");

            // 2. Mevcut veriyi dönüştür: GridColumnsX/GridRowsY → JSON LayoutDefinition
            // Her mevcut düzen için eşit boyutlu satır ve sütunlarla JSON oluştur
            migrationBuilder.Sql(@"
                UPDATE Layouts
                SET LayoutDefinition = (
                    SELECT '{""rows"":[' +
                        STUFF((
                            SELECT ',' + '{""height"":' + CAST(CAST(100.0 / GridRowsY AS INT) AS NVARCHAR(10)) + ',""columns"":[' +
                                STUFF((
                                    SELECT ',' + '{""width"":' + CAST(CAST(100.0 / GridColumnsX AS INT) AS NVARCHAR(10)) + '}'
                                    FROM (SELECT TOP (GridColumnsX) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.objects) cols
                                    FOR XML PATH(''), TYPE
                                ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') + ']}'
                            FROM (SELECT TOP (GridRowsY) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.objects) rows
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') + ']}'
                )
                WHERE GridColumnsX IS NOT NULL AND GridRowsY IS NOT NULL
            ");

            // 3. Eski sütunları kaldır
            migrationBuilder.DropColumn(
                name: "GridColumnsX",
                table: "Layouts");

            migrationBuilder.DropColumn(
                name: "GridRowsY",
                table: "Layouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GridColumnsX",
                table: "Layouts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "GridRowsY",
                table: "Layouts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.DropColumn(
                name: "LayoutDefinition",
                table: "Layouts");
        }
    }
}
