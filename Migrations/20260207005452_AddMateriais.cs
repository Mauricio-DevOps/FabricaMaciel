using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Migrations
{
    /// <inheritdoc />
    public partial class AddMateriais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acessorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PesoUnitarioKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acessorio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaioMm = table.Column<int>(type: "INTEGER", nullable: false),
                    GrossuraMm = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    PesoUnitarioKg = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disco", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acessorio");

            migrationBuilder.DropTable(
                name: "Disco");
        }
    }
}
