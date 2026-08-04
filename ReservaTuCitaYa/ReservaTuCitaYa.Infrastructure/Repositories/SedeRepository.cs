using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories
{
    public class SedeRepository : ISedeRepository
    {
        private readonly ApplicationDbContext _context;

        public SedeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<SedeListaDto>> ListarAsync(
            SedeFiltroDto filtro,
            CancellationToken cancellationToken = default)
        {
            var consulta = _context.Sedes
                .AsNoTracking()
                .Where(sede => sede.OrganizacionId == filtro.OrganizacionId);

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                var busqueda = filtro.Busqueda.Trim();
                consulta = consulta.Where(sede =>
                    sede.Nombre.Contains(busqueda) || sede.Direccion.Contains(busqueda));
            }

            consulta = filtro.Estado switch
            {
                EstadoFiltro.Activos => consulta.Where(sede => sede.EstaActivo),
                EstadoFiltro.Inactivos => consulta.Where(sede => !sede.EstaActivo),
                _ => consulta
            };

            return await consulta
                .OrderBy(sede => sede.Nombre)
                .Select(sede => new SedeListaDto(
                    sede.Id,
                    sede.OrganizacionId,
                    sede.Nombre,
                    sede.Direccion,
                    sede.Telefono,
                    sede.Correo,
                    sede.Referencia,
                    sede.EstaActivo))
                .ToListAsync(cancellationToken);
        }

        public Task<SedeDetalleDto?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _context.Sedes
                .AsNoTracking()
                .Where(sede => sede.Id == id)
                .Select(sede => new SedeDetalleDto(
                    sede.Id,
                    sede.OrganizacionId,
                    sede.Organizacion.NombreComercial,
                    sede.Nombre,
                    sede.Direccion,
                    sede.Telefono,
                    sede.Correo,
                    sede.Referencia,
                    sede.EstaActivo,
                    sede.FechaCreacion,
                    sede.FechaModificacion))
                .SingleOrDefaultAsync(cancellationToken);

        public Task<Sede?> ObtenerParaModificarAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _context.Sedes
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(sede => sede.Id == id, cancellationToken);

        public Task<bool> ExisteNombreActivoAsync(
            Guid organizacionId,
            string nombre,
            Guid? excluirId = null,
            CancellationToken cancellationToken = default) =>
            _context.Sedes.AnyAsync(
                sede => sede.OrganizacionId == organizacionId &&
                        sede.Nombre == nombre &&
                        sede.EstaActivo &&
                        (!excluirId.HasValue || sede.Id != excluirId.Value),
                cancellationToken);

        public void Agregar(Sede sede) => _context.Sedes.Add(sede);

        public async Task GuardarAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictoPersistenciaException(
                    "La sede entra en conflicto con un registro existente.",
                    exception);
            }
        }
    }
}
