using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class Rg014MigrationTests
{
    [Fact]
    public async Task Migraciones_CreanLasSeisTablasDeRecursosYDisponibilidad()
    {
        var databaseName = $"ReservaTuCitaYa_RG014_{Guid.NewGuid():N}";
        var connectionString =
            $"Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);

        try
        {
            await context.Database.MigrateAsync();
            await context.Database.OpenConnectionAsync();

            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM sys.tables
                WHERE name IN (
                    'Recursos',
                    'HorariosRecursos',
                    'HorariosSede',
                    'HorariosProfesionales',
                    'BloqueosRecursos',
                    'BloqueosProfesionales')
                """;

            var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
            Assert.Equal(6, tableCount);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
            await context.Database.EnsureDeletedAsync();
        }
    }
}
