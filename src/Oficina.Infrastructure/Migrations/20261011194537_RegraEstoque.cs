using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oficina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RegraEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Consumido",
                table: "ItensPecaInsumo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Estoques.xmin (token de concorrência) é uma coluna de sistema do PostgreSQL:
            // já existe em toda tabela, por isso não é criada aqui.

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Consumido",
                table: "ItensPecaInsumo");

        }
    }
}
