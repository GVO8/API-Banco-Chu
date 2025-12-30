using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMPTec.Infrastructure.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroConta = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Agencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoConta = table.Column<int>(type: "int", nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataEncerramento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conta_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transferencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CodigoRastreio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComprovanteUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Taxa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ContaOrigemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transferencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transferencia_Conta_ContaDestinoId",
                        column: x => x.ContaDestinoId,
                        principalTable: "Conta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transferencia_Conta_ContaOrigemId",
                        column: x => x.ContaOrigemId,
                        principalTable: "Conta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_CPF",
                table: "Cliente",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Email",
                table: "Cliente",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conta_ClienteId",
                table: "Conta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Conta_NumeroConta",
                table: "Conta",
                column: "NumeroConta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transferencia_ContaDestinoId",
                table: "Transferencia",
                column: "ContaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferencia_ContaOrigemId",
                table: "Transferencia",
                column: "ContaOrigemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transferencia");

            migrationBuilder.DropTable(
                name: "Conta");

            migrationBuilder.DropTable(
                name: "Cliente");
        }
    }
}
