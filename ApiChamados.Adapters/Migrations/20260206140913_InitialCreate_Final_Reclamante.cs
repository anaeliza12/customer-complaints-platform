using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiChamados.Adapters.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Final_Reclamante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Cpf = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reclamacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    SLAViolado = table.Column<bool>(type: "boolean", nullable: false),
                    UltimaNotificacaoSLA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerfilCliente_Cpf = table.Column<string>(type: "text", nullable: false),
                    PerfilCliente_ScoreCredito = table.Column<int>(type: "integer", nullable: false),
                    PerfilCliente_RelacionamentoAnos = table.Column<int>(type: "integer", nullable: false),
                    PerfilCliente_Produtos = table.Column<string>(type: "jsonb", nullable: false),
                    PerfilCliente_Risco = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reclamacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reclamacoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anexos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    NomeArquivo = table.Column<string>(type: "text", nullable: false),
                    ReclamacaoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anexos_Reclamacoes_ReclamacaoId",
                        column: x => x.ReclamacaoId,
                        principalTable: "Reclamacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    ReclamacaoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Reclamacoes_ReclamacaoId",
                        column: x => x.ReclamacaoId,
                        principalTable: "Reclamacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anexos_ReclamacaoId",
                table: "Anexos",
                column: "ReclamacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_ReclamacaoId",
                table: "Categorias",
                column: "ReclamacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamacoes_ClienteId",
                table: "Reclamacoes",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anexos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Reclamacoes");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
