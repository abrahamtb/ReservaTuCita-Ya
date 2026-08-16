using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHorariosYExcepciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFinVigencia",
                table: "HorariosProfesionales");

            migrationBuilder.DropColumn(
                name: "FechaInicioVigencia",
                table: "HorariosProfesionales");

            migrationBuilder.RenameColumn(
                name: "ProfesionalId",
                table: "HorariosProfesionales",
                newName: "EmpleadoId");

            migrationBuilder.CreateTable(
                name: "ExcepcionesHorarioRecurso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TipoExcepcion = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionesHorarioRecurso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionesHorarioRecurso_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcepcionHorarioProfesional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TipoExcepcion = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionHorarioProfesional", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionHorarioProfesional_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExcepcionHorarioProfesional_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcepcionHorarioSede",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TipoExcepcion = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionHorarioSede", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionHorarioSede_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosProfesionales_EmpleadoId",
                table: "HorariosProfesionales",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorarioRecurso_RecursoId",
                table: "ExcepcionesHorarioRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioProfesional_EmpleadoId",
                table: "ExcepcionHorarioProfesional",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioProfesional_SedeId",
                table: "ExcepcionHorarioProfesional",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioSede_SedeId",
                table: "ExcepcionHorarioSede",
                column: "SedeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosProfesionales_Empleados_EmpleadoId",
                table: "HorariosProfesionales",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorariosProfesionales_Empleados_EmpleadoId",
                table: "HorariosProfesionales");

            migrationBuilder.DropTable(
                name: "ExcepcionesHorarioRecurso");

            migrationBuilder.DropTable(
                name: "ExcepcionHorarioProfesional");

            migrationBuilder.DropTable(
                name: "ExcepcionHorarioSede");

            migrationBuilder.DropIndex(
                name: "IX_HorariosProfesionales_EmpleadoId",
                table: "HorariosProfesionales");

            migrationBuilder.RenameColumn(
                name: "EmpleadoId",
                table: "HorariosProfesionales",
                newName: "ProfesionalId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaFinVigencia",
                table: "HorariosProfesionales",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaInicioVigencia",
                table: "HorariosProfesionales",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
