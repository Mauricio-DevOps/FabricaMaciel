using System;
using Fabrica.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260407234500_AddEstoqueMovimentos")]
    public partial class AddEstoqueMovimentos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstoqueMovimento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Operacao = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AcessorioId = table.Column<int>(type: "INTEGER", nullable: true),
                    DiscoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantidade = table.Column<decimal>(type: "TEXT", precision: 14, scale: 4, nullable: false),
                    ConsumoAutomatico = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    DataCriacaoUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueMovimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstoqueMovimento_Acessorio_AcessorioId",
                        column: x => x.AcessorioId,
                        principalTable: "Acessorio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstoqueMovimento_Disco_DiscoId",
                        column: x => x.DiscoId,
                        principalTable: "Disco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstoqueMovimento_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimento_AcessorioId",
                table: "EstoqueMovimento",
                column: "AcessorioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimento_DataCriacaoUtc",
                table: "EstoqueMovimento",
                column: "DataCriacaoUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimento_DiscoId",
                table: "EstoqueMovimento",
                column: "DiscoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimento_ItemId",
                table: "EstoqueMovimento",
                column: "ItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstoqueMovimento");
        }
    }
}
