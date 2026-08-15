using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRG020RG024Integracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloqueosRecursos_Recursos_RecursoId1",
                table: "BloqueosRecursos");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionesHorarioRecurso_Recursos_RecursoId",
                table: "ExcepcionesHorarioRecurso");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Empleados_EmpleadoId",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Sedes_SedeId",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioSede_Sedes_SedeId",
                table: "ExcepcionHorarioSede");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosRecurso_Recursos_RecursoId",
                table: "ServiciosRecurso");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosRecurso_Servicios_ServicioId",
                table: "ServiciosRecurso");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosRecurso_ServicioId",
                table: "ServiciosRecurso");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_SedeId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionHorarioSede_SedeId",
                table: "ExcepcionHorarioSede");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionHorarioProfesional_EmpleadoId",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionesHorarioRecurso_RecursoId",
                table: "ExcepcionesHorarioRecurso");

            migrationBuilder.DropIndex(
                name: "IX_BloqueosRecursos_RecursoId",
                table: "BloqueosRecursos");

            migrationBuilder.DropIndex(
                name: "IX_BloqueosRecursos_RecursoId1",
                table: "BloqueosRecursos");

            migrationBuilder.DropColumn(
                name: "RecursoId1",
                table: "BloqueosRecursos");

            migrationBuilder.AlterColumn<int>(
                name: "CantidadRequerida",
                table: "ServiciosRecurso",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "TipoRecurso",
                table: "Recursos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Recursos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Recursos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionHorarioSede",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionHorarioSede",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionHorarioProfesional",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionHorarioProfesional",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionesHorarioRecurso",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionesHorarioRecurso",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfesionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinServicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraInicioOcupacion = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinOcupacion = table.Column<TimeOnly>(type: "time", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    TiempoPreparacionMinutos = table.Column<int>(type: "int", nullable: false),
                    TiempoPosteriorMinutos = table.Column<int>(type: "int", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdelantoRequerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EsGrupal = table.Column<bool>(type: "bit", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: false),
                    CantidadParticipantes = table.Column<int>(type: "int", nullable: false),
                    EstadoReserva = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Empleados_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CancelacionesReserva",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PoliticaAplicada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelacionesReserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancelacionesReserva_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialReservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    TipoAccion = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaAccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialReservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialReservas_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReprogramacionesReserva",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaAnterior = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicioAnterior = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinServicioAnterior = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraInicioOcupacionAnterior = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinOcupacionAnterior = table.Column<TimeOnly>(type: "time", nullable: false),
                    ProfesionalAnteriorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecursoAnteriorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaNueva = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicioNueva = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinServicioNueva = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraInicioOcupacionNueva = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFinOcupacionNueva = table.Column<TimeOnly>(type: "time", nullable: false),
                    ProfesionalNuevoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecursoNuevoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Motivo = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaReprogramacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprogramacionesReserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesReserva_Empleados_ProfesionalAnteriorId",
                        column: x => x.ProfesionalAnteriorId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesReserva_Empleados_ProfesionalNuevoId",
                        column: x => x.ProfesionalNuevoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesReserva_Recursos_RecursoAnteriorId",
                        column: x => x.RecursoAnteriorId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesReserva_Recursos_RecursoNuevoId",
                        column: x => x.RecursoNuevoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesReserva_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservaParticipantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EsTitular = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaParticipantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservaParticipantes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservaParticipantes_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosRecurso_ServicioId_RecursoId",
                table: "ServiciosRecurso",
                columns: new[] { "ServicioId", "RecursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SedeId_Codigo",
                table: "Recursos",
                columns: new[] { "SedeId", "Codigo" },
                unique: true,
                filter: "[Codigo] IS NOT NULL AND [EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioSede_SedeId_Fecha",
                table: "ExcepcionHorarioSede",
                columns: new[] { "SedeId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioProfesional_EmpleadoId_SedeId_Fecha",
                table: "ExcepcionHorarioProfesional",
                columns: new[] { "EmpleadoId", "SedeId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorarioRecurso_RecursoId_Fecha",
                table: "ExcepcionesHorarioRecurso",
                columns: new[] { "RecursoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRecursos_RecursoId_FechaHoraInicio_FechaHoraFin",
                table: "BloqueosRecursos",
                columns: new[] { "RecursoId", "FechaHoraInicio", "FechaHoraFin" });

            migrationBuilder.CreateIndex(
                name: "IX_CancelacionesReserva_ReservaId",
                table: "CancelacionesReserva",
                column: "ReservaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialReservas_ReservaId_FechaAccion",
                table: "HistorialReservas",
                columns: new[] { "ReservaId", "FechaAccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesReserva_ProfesionalAnteriorId",
                table: "ReprogramacionesReserva",
                column: "ProfesionalAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesReserva_ProfesionalNuevoId",
                table: "ReprogramacionesReserva",
                column: "ProfesionalNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesReserva_RecursoAnteriorId",
                table: "ReprogramacionesReserva",
                column: "RecursoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesReserva_RecursoNuevoId",
                table: "ReprogramacionesReserva",
                column: "RecursoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesReserva_ReservaId_FechaReprogramacion",
                table: "ReprogramacionesReserva",
                columns: new[] { "ReservaId", "FechaReprogramacion" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservaParticipantes_ClienteId",
                table: "ReservaParticipantes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservaParticipantes_ReservaId",
                table: "ReservaParticipantes",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ClienteId",
                table: "Reservas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Codigo",
                table: "Reservas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_OrganizacionId_Fecha_HoraInicio",
                table: "Reservas",
                columns: new[] { "OrganizacionId", "Fecha", "HoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ProfesionalId_Fecha_HoraInicioOcupacion_HoraFinOcupacion",
                table: "Reservas",
                columns: new[] { "ProfesionalId", "Fecha", "HoraInicioOcupacion", "HoraFinOcupacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_RecursoId_Fecha_HoraInicioOcupacion_HoraFinOcupacion",
                table: "Reservas",
                columns: new[] { "RecursoId", "Fecha", "HoraInicioOcupacion", "HoraFinOcupacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SedeId",
                table: "Reservas",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ServicioId",
                table: "Reservas",
                column: "ServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionesHorarioRecurso_Recursos_RecursoId",
                table: "ExcepcionesHorarioRecurso",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Empleados_EmpleadoId",
                table: "ExcepcionHorarioProfesional",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Sedes_SedeId",
                table: "ExcepcionHorarioProfesional",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioSede_Sedes_SedeId",
                table: "ExcepcionHorarioSede",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosRecurso_Recursos_RecursoId",
                table: "ServiciosRecurso",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosRecurso_Servicios_ServicioId",
                table: "ServiciosRecurso",
                column: "ServicioId",
                principalTable: "Servicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionesHorarioRecurso_Recursos_RecursoId",
                table: "ExcepcionesHorarioRecurso");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Empleados_EmpleadoId",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Sedes_SedeId",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionHorarioSede_Sedes_SedeId",
                table: "ExcepcionHorarioSede");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosRecurso_Recursos_RecursoId",
                table: "ServiciosRecurso");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosRecurso_Servicios_ServicioId",
                table: "ServiciosRecurso");

            migrationBuilder.DropTable(
                name: "CancelacionesReserva");

            migrationBuilder.DropTable(
                name: "HistorialReservas");

            migrationBuilder.DropTable(
                name: "ReprogramacionesReserva");

            migrationBuilder.DropTable(
                name: "ReservaParticipantes");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosRecurso_ServicioId_RecursoId",
                table: "ServiciosRecurso");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_SedeId_Codigo",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionHorarioSede_SedeId_Fecha",
                table: "ExcepcionHorarioSede");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionHorarioProfesional_EmpleadoId_SedeId_Fecha",
                table: "ExcepcionHorarioProfesional");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionesHorarioRecurso_RecursoId_Fecha",
                table: "ExcepcionesHorarioRecurso");

            migrationBuilder.DropIndex(
                name: "IX_BloqueosRecursos_RecursoId_FechaHoraInicio_FechaHoraFin",
                table: "BloqueosRecursos");

            migrationBuilder.AlterColumn<int>(
                name: "CantidadRequerida",
                table: "ServiciosRecurso",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "TipoRecurso",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionHorarioSede",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionHorarioSede",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionHorarioProfesional",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionHorarioProfesional",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "ExcepcionesHorarioRecurso",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "ExcepcionesHorarioRecurso",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<Guid>(
                name: "RecursoId1",
                table: "BloqueosRecursos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosRecurso_ServicioId",
                table: "ServiciosRecurso",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SedeId",
                table: "Recursos",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioSede_SedeId",
                table: "ExcepcionHorarioSede",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionHorarioProfesional_EmpleadoId",
                table: "ExcepcionHorarioProfesional",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorarioRecurso_RecursoId",
                table: "ExcepcionesHorarioRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRecursos_RecursoId",
                table: "BloqueosRecursos",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRecursos_RecursoId1",
                table: "BloqueosRecursos",
                column: "RecursoId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BloqueosRecursos_Recursos_RecursoId1",
                table: "BloqueosRecursos",
                column: "RecursoId1",
                principalTable: "Recursos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionesHorarioRecurso_Recursos_RecursoId",
                table: "ExcepcionesHorarioRecurso",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Empleados_EmpleadoId",
                table: "ExcepcionHorarioProfesional",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioProfesional_Sedes_SedeId",
                table: "ExcepcionHorarioProfesional",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionHorarioSede_Sedes_SedeId",
                table: "ExcepcionHorarioSede",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosRecurso_Recursos_RecursoId",
                table: "ServiciosRecurso",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosRecurso_Servicios_ServicioId",
                table: "ServiciosRecurso",
                column: "ServicioId",
                principalTable: "Servicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
