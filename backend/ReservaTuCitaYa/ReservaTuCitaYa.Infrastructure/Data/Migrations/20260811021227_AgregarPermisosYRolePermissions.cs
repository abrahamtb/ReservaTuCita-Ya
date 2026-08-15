using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaTuCitaYa.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AgregarPermisosYRolePermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Esta migración fue creada originalmente al renombrar la migración compartida
        // AgregarEmpleadosProfesionales. Permissions, RolePermissions y los vínculos de
        // usuario ya pertenecen a migraciones anteriores y no deben crearse nuevamente.
        // Las comprobaciones permiten reconciliar bases que ya aplicaron el ID original.
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[Empleados]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Empleados]
                (
                    [Id] uniqueidentifier NOT NULL,
                    [OrganizacionId] uniqueidentifier NOT NULL,
                    [TipoDocumento] int NOT NULL,
                    [NumeroDocumento] nvarchar(20) NOT NULL,
                    [Nombres] nvarchar(100) NOT NULL,
                    [Apellidos] nvarchar(100) NOT NULL,
                    [Correo] nvarchar(150) NULL,
                    [Telefono] nvarchar(30) NULL,
                    [Direccion] nvarchar(250) NULL,
                    [FechaNacimiento] date NULL,
                    [Cargo] nvarchar(100) NOT NULL,
                    [Especialidad] nvarchar(150) NULL,
                    [EsProfesional] bit NOT NULL,
                    [NumeroColegiatura] nvarchar(50) NULL,
                    [Observaciones] nvarchar(500) NULL,
                    [FechaCreacion] datetime2 NOT NULL,
                    [FechaModificacion] datetime2 NULL,
                    [CreadoPorUsuarioId] uniqueidentifier NULL,
                    [ModificadoPorUsuarioId] uniqueidentifier NULL,
                    [EstaActivo] bit NOT NULL,
                    [EstaEliminado] bit NOT NULL,
                    CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Empleados_Organizaciones_OrganizacionId]
                        FOREIGN KEY ([OrganizacionId]) REFERENCES [dbo].[Organizaciones] ([Id])
                        ON DELETE NO ACTION
                );

                EXEC sys.sp_addextendedproperty
                    @name = N'RG030_CreadaPor_20260811021227', @value = 1,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = N'Empleados';
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_Empleados_OrganizacionId'
                  AND [object_id] = OBJECT_ID(N'[dbo].[Empleados]'))
                CREATE INDEX [IX_Empleados_OrganizacionId]
                    ON [dbo].[Empleados] ([OrganizacionId]);

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_Empleados_OrganizacionId_TipoDocumento_NumeroDocumento'
                  AND [object_id] = OBJECT_ID(N'[dbo].[Empleados]'))
                CREATE UNIQUE INDEX [IX_Empleados_OrganizacionId_TipoDocumento_NumeroDocumento]
                    ON [dbo].[Empleados] ([OrganizacionId], [TipoDocumento], [NumeroDocumento]);

            IF OBJECT_ID(N'[dbo].[EmpleadosSede]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[EmpleadosSede]
                (
                    [Id] uniqueidentifier NOT NULL,
                    [EmpleadoId] uniqueidentifier NOT NULL,
                    [SedeId] uniqueidentifier NOT NULL,
                    [FechaCreacion] datetime2 NOT NULL,
                    [FechaModificacion] datetime2 NULL,
                    [CreadoPorUsuarioId] uniqueidentifier NULL,
                    [ModificadoPorUsuarioId] uniqueidentifier NULL,
                    [EstaActivo] bit NOT NULL,
                    [EstaEliminado] bit NOT NULL,
                    CONSTRAINT [PK_EmpleadosSede] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_EmpleadosSede_Empleados_EmpleadoId]
                        FOREIGN KEY ([EmpleadoId]) REFERENCES [dbo].[Empleados] ([Id])
                        ON DELETE NO ACTION,
                    CONSTRAINT [FK_EmpleadosSede_Sedes_SedeId]
                        FOREIGN KEY ([SedeId]) REFERENCES [dbo].[Sedes] ([Id])
                        ON DELETE NO ACTION
                );

                EXEC sys.sp_addextendedproperty
                    @name = N'RG030_CreadaPor_20260811021227', @value = 1,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = N'EmpleadosSede';
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_EmpleadosSede_EmpleadoId_SedeId'
                  AND [object_id] = OBJECT_ID(N'[dbo].[EmpleadosSede]'))
                CREATE UNIQUE INDEX [IX_EmpleadosSede_EmpleadoId_SedeId]
                    ON [dbo].[EmpleadosSede] ([EmpleadoId], [SedeId]);

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_EmpleadosSede_SedeId'
                  AND [object_id] = OBJECT_ID(N'[dbo].[EmpleadosSede]'))
                CREATE INDEX [IX_EmpleadosSede_SedeId]
                    ON [dbo].[EmpleadosSede] ([SedeId]);

            IF OBJECT_ID(N'[dbo].[ProfesionalesServicio]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ProfesionalesServicio]
                (
                    [Id] uniqueidentifier NOT NULL,
                    [EmpleadoId] uniqueidentifier NOT NULL,
                    [ServicioId] uniqueidentifier NOT NULL,
                    [FechaCreacion] datetime2 NOT NULL,
                    [FechaModificacion] datetime2 NULL,
                    [CreadoPorUsuarioId] uniqueidentifier NULL,
                    [ModificadoPorUsuarioId] uniqueidentifier NULL,
                    [EstaActivo] bit NOT NULL,
                    [EstaEliminado] bit NOT NULL,
                    CONSTRAINT [PK_ProfesionalesServicio] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_ProfesionalesServicio_Empleados_EmpleadoId]
                        FOREIGN KEY ([EmpleadoId]) REFERENCES [dbo].[Empleados] ([Id])
                        ON DELETE NO ACTION,
                    CONSTRAINT [FK_ProfesionalesServicio_Servicios_ServicioId]
                        FOREIGN KEY ([ServicioId]) REFERENCES [dbo].[Servicios] ([Id])
                        ON DELETE NO ACTION
                );

                EXEC sys.sp_addextendedproperty
                    @name = N'RG030_CreadaPor_20260811021227', @value = 1,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = N'ProfesionalesServicio';
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_ProfesionalesServicio_EmpleadoId_ServicioId'
                  AND [object_id] = OBJECT_ID(N'[dbo].[ProfesionalesServicio]'))
                CREATE UNIQUE INDEX [IX_ProfesionalesServicio_EmpleadoId_ServicioId]
                    ON [dbo].[ProfesionalesServicio] ([EmpleadoId], [ServicioId]);

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_ProfesionalesServicio_ServicioId'
                  AND [object_id] = OBJECT_ID(N'[dbo].[ProfesionalesServicio]'))
                CREATE INDEX [IX_ProfesionalesServicio_ServicioId]
                    ON [dbo].[ProfesionalesServicio] ([ServicioId]);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[EmpleadosSede]', N'U') IS NOT NULL
               AND EXISTS (
                   SELECT 1 FROM sys.extended_properties
                   WHERE [major_id] = OBJECT_ID(N'[dbo].[EmpleadosSede]')
                     AND [name] = N'RG030_CreadaPor_20260811021227')
                DROP TABLE [dbo].[EmpleadosSede];

            IF OBJECT_ID(N'[dbo].[ProfesionalesServicio]', N'U') IS NOT NULL
               AND EXISTS (
                   SELECT 1 FROM sys.extended_properties
                   WHERE [major_id] = OBJECT_ID(N'[dbo].[ProfesionalesServicio]')
                     AND [name] = N'RG030_CreadaPor_20260811021227')
                DROP TABLE [dbo].[ProfesionalesServicio];

            IF OBJECT_ID(N'[dbo].[Empleados]', N'U') IS NOT NULL
               AND EXISTS (
                   SELECT 1 FROM sys.extended_properties
                   WHERE [major_id] = OBJECT_ID(N'[dbo].[Empleados]')
                     AND [name] = N'RG030_CreadaPor_20260811021227')
                DROP TABLE [dbo].[Empleados];
            """);
    }
}
