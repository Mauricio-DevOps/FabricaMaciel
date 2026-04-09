using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Migrations
{
    public partial class AddItens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: true),
                    DiscoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PossuiTampa = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiscoTampaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Disco_DiscoId",
                        column: x => x.DiscoId,
                        principalTable: "Disco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Disco_DiscoTampaId",
                        column: x => x.DiscoTampaId,
                        principalTable: "Disco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemAcessorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcessorioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAcessorio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemAcessorio_Acessorio_AcessorioId",
                        column: x => x.AcessorioId,
                        principalTable: "Acessorio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemAcessorio_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_DiscoId",
                table: "Item",
                column: "DiscoId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_DiscoTampaId",
                table: "Item",
                column: "DiscoTampaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAcessorio_AcessorioId",
                table: "ItemAcessorio",
                column: "AcessorioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAcessorio_ItemId_AcessorioId",
                table: "ItemAcessorio",
                columns: new[] { "ItemId", "AcessorioId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemAcessorio");

            migrationBuilder.DropTable(
                name: "Item");
        }
    }
}
