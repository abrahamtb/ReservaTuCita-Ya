using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecursosHorariosDisponibilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloqueosProfesionales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfesionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoBloqueo = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosProfesionales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorariosProfesionales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfesionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    FechaInicioVigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFinVigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosProfesionales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosProfesionales_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HorariosSede",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosSede", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosSede_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UbicacionInterna = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    EstadoRecurso = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recursos_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recursos_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BloqueosRecursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoBloqueo = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosRecursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueosRecursos_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HorariosRecursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosRecursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosRecursos_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloqueosRecursos_RecursoId",
                table: "BloqueosRecursos",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosProfesionales_SedeId",
                table: "HorariosProfesionales",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosRecursos_RecursoId",
                table: "HorariosRecursos",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosSede_SedeId",
                table: "HorariosSede",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_OrganizacionId",
                table: "Recursos",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SedeId",
                table: "Recursos",
                column: "SedeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BloqueosProfesionales");
            migrationBuilder.DropTable(name: "BloqueosRecursos");
            migrationBuilder.DropTable(name: "HorariosProfesionales");
            migrationBuilder.DropTable(name: "HorariosRecursos");
            migrationBuilder.DropTable(name: "HorariosSede");
            migrationBuilder.DropTable(name: "Recursos");
        }
    }
}
