using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalCopa26.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estadios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Capacidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estadios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Simulacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CriadaEmUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadaEmUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Selecoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoFifa = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Tecnico = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CabecaDeChave = table.Column<bool>(type: "INTEGER", nullable: false),
                    GrupoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selecoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Selecoes_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Posicao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Idade = table.Column<int>(type: "INTEGER", nullable: false),
                    Gols = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipacoesCopas = table.Column<int>(type: "INTEGER", nullable: false),
                    SelecaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadores_Selecoes_SelecaoId",
                        column: x => x.SelecaoId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Fase = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Rotulo = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    DataHoraUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GrupoId = table.Column<int>(type: "INTEGER", nullable: true),
                    EstadioId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelecaoMandanteId = table.Column<int>(type: "INTEGER", nullable: true),
                    SelecaoVisitanteId = table.Column<int>(type: "INTEGER", nullable: true),
                    DescricaoMandante = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    DescricaoVisitante = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    GolsMandante = table.Column<int>(type: "INTEGER", nullable: true),
                    GolsVisitante = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogos_Estadios_EstadioId",
                        column: x => x.EstadioId,
                        principalTable: "Estadios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Selecoes_SelecaoMandanteId",
                        column: x => x.SelecaoMandanteId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Selecoes_SelecaoVisitanteId",
                        column: x => x.SelecaoVisitanteId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankingFifa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    NomeSelecao = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Pontos = table.Column<decimal>(type: "TEXT", precision: 7, scale: 2, nullable: false),
                    SelecaoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingFifa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingFifa_Selecoes_SelecaoId",
                        column: x => x.SelecaoId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimulacaoJogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SimulacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    JogoId = table.Column<int>(type: "INTEGER", nullable: false),
                    GolsMandante = table.Column<int>(type: "INTEGER", nullable: false),
                    GolsVisitante = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulacaoJogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulacaoJogos_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimulacaoJogos_Simulacoes_SimulacaoId",
                        column: x => x.SimulacaoId,
                        principalTable: "Simulacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_Codigo",
                table: "Grupos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_SelecaoId",
                table: "Jogadores",
                column: "SelecaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_DataHoraUtc",
                table: "Jogos",
                column: "DataHoraUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_EstadioId",
                table: "Jogos",
                column: "EstadioId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_GrupoId",
                table: "Jogos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Numero",
                table: "Jogos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_SelecaoMandanteId",
                table: "Jogos",
                column: "SelecaoMandanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_SelecaoVisitanteId",
                table: "Jogos",
                column: "SelecaoVisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingFifa_Posicao",
                table: "RankingFifa",
                column: "Posicao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingFifa_SelecaoId",
                table: "RankingFifa",
                column: "SelecaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selecoes_CodigoFifa",
                table: "Selecoes",
                column: "CodigoFifa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selecoes_GrupoId",
                table: "Selecoes",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulacaoJogos_JogoId",
                table: "SimulacaoJogos",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulacaoJogos_SimulacaoId_JogoId",
                table: "SimulacaoJogos",
                columns: new[] { "SimulacaoId", "JogoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "RankingFifa");

            migrationBuilder.DropTable(
                name: "SimulacaoJogos");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropTable(
                name: "Simulacoes");

            migrationBuilder.DropTable(
                name: "Estadios");

            migrationBuilder.DropTable(
                name: "Selecoes");

            migrationBuilder.DropTable(
                name: "Grupos");
        }
    }
}
