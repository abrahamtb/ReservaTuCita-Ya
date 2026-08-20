using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RG020_RecursosYBloqueos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoRecurso",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "BloqueosRecursos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecursoId1",
                table: "BloqueosRecursos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiciosRecurso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    CantidadRequerida = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosRecurso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiciosRecurso_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiciosRecurso_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRecursos_RecursoId1",
                table: "BloqueosRecursos",
                column: "RecursoId1");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosRecurso_RecursoId",
                table: "ServiciosRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosRecurso_ServicioId",
                table: "ServiciosRecurso",
                column: "ServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloqueosRecursos_Recursos_RecursoId1",
                table: "BloqueosRecursos",
                column: "RecursoId1",
                principalTable: "Recursos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloqueosRecursos_Recursos_RecursoId1",
                table: "BloqueosRecursos");

            migrationBuilder.DropTable(
                name: "ServiciosRecurso");

            migrationBuilder.DropIndex(
                name: "IX_BloqueosRecursos_RecursoId1",
                table: "BloqueosRecursos");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "TipoRecurso",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "BloqueosRecursos");

            migrationBuilder.DropColumn(
                name: "RecursoId1",
                table: "BloqueosRecursos");
        }
    }
}
