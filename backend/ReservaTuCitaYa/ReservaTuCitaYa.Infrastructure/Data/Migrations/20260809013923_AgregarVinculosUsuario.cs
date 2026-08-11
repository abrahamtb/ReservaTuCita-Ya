using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVinculosUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosClientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosEmpleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosEmpleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosOrganizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosOrganizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosOrganizaciones_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosClientes_UsuarioId_ClienteId",
                table: "UsuariosClientes",
                columns: new[] { "UsuarioId", "ClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosEmpleados_UsuarioId_EmpleadoId",
                table: "UsuariosEmpleados",
                columns: new[] { "UsuarioId", "EmpleadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosOrganizaciones_OrganizacionId",
                table: "UsuariosOrganizaciones",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosOrganizaciones_UsuarioId_OrganizacionId",
                table: "UsuariosOrganizaciones",
                columns: new[] { "UsuarioId", "OrganizacionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosClientes");

            migrationBuilder.DropTable(
                name: "UsuariosEmpleados");

            migrationBuilder.DropTable(
                name: "UsuariosOrganizaciones");
        }
    }
}
