using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations;

public partial class AgregarCalificacionesRG029 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Calificaciones",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AtencionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Puntuacion = table.Column<int>(type: "int", nullable: false),
                Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                FechaCalificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Calificaciones", x => x.Id);
                table.CheckConstraint("CK_Calificaciones_Puntuacion", "[Puntuacion] >= 1 AND [Puntuacion] <= 5");
                table.ForeignKey(
                    name: "FK_Calificaciones_Atenciones_AtencionId",
                    column: x => x.AtencionId,
                    principalTable: "Atenciones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Calificaciones_Reservas_ReservaId",
                    column: x => x.ReservaId,
                    principalTable: "Reservas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Calificaciones_AtencionId",
            table: "Calificaciones",
            column: "AtencionId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Calificaciones_ReservaId",
            table: "Calificaciones",
            column: "ReservaId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Calificaciones");
    }
}
