using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oficina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDocumentoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoNumero",
                table: "Clientes",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "DocumentoTipo",
                table: "Clientes",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_DocumentoNumero",
                table: "Clientes",
                column: "DocumentoNumero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_DocumentoNumero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DocumentoNumero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DocumentoTipo",
                table: "Clientes");
        }
    }
}
