using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelaPrecoEPrecosItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoAtacado",
                table: "Item",
                type: "TEXT",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoPromocional",
                table: "Item",
                type: "TEXT",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoVarejo",
                table: "Item",
                type: "TEXT",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TabelaPreco",
                table: "Cliente",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "Varejo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoAtacado",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "PrecoPromocional",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "PrecoVarejo",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "TabelaPreco",
                table: "Cliente");
        }
    }
}
