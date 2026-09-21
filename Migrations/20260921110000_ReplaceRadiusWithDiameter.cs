using Fabrica.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260921110000_ReplaceRadiusWithDiameter")]
public partial class ReplaceRadiusWithDiameter : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "RaioMm",
            table: "Disco",
            newName: "DiametroMm");

        migrationBuilder.Sql("UPDATE \"Disco\" SET \"DiametroMm\" = \"DiametroMm\" * 2;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE \"Disco\" SET \"DiametroMm\" = \"DiametroMm\" / 2;");

        migrationBuilder.RenameColumn(
            name: "DiametroMm",
            table: "Disco",
            newName: "RaioMm");
    }
}
