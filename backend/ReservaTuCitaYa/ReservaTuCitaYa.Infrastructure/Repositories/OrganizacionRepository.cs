using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories
{
    public class OrganizacionRepository : IOrganizacionRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default)
        {
            return await CrearConsulta(filtro).ToListAsync(cancellationToken);
        }

        public async Task<PaginaResultado<OrganizacionListaDto>> ListarPaginadoAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default)
        {
            var pagina = Math.Max(1, filtro.Pagina);
            var tamano = Math.Clamp(filtro.TamanoPagina, 1, 50);
            var consulta = CrearConsulta(filtro);
            var total = await consulta.CountAsync(cancellationToken);
            var elementos = await consulta
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync(cancellationToken);

            return new PaginaResultado<OrganizacionListaDto>(elementos, pagina, tamano, total);
        }

        private IQueryable<OrganizacionListaDto> CrearConsulta(OrganizacionFiltroDto filtro)
        {
            var consulta = _context.Organizaciones.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                var busqueda = filtro.Busqueda.Trim();
                consulta = consulta.Where(organizacion =>
                    organizacion.NombreComercial.Contains(busqueda) ||
                    (organizacion.RazonSocial != null && organizacion.RazonSocial.Contains(busqueda)) ||
                    organizacion.NumeroDocumento.Contains(busqueda));
            }

            consulta = filtro.Estado switch
            {
                EstadoFiltro.Activos => consulta.Where(organizacion => organizacion.EstaActivo),
                EstadoFiltro.Inactivos => consulta.Where(organizacion => !organizacion.EstaActivo),
                _ => consulta
            };

            return consulta
                .OrderBy(organizacion => organizacion.NombreComercial)
                .Select(organizacion => new OrganizacionListaDto(
                    organizacion.Id,
                    organizacion.NombreComercial,
                    organizacion.RazonSocial,
                    organizacion.NumeroDocumento,
                    organizacion.TipoOrganizacion.Nombre,
                    organizacion.Telefono,
                    organizacion.Correo,
                    organizacion.EstaActivo));
        }

        public Task<OrganizacionDetalleDto?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _context.Organizaciones
                .AsNoTracking()
                .Where(organizacion => organizacion.Id == id)
                .Select(organizacion => new OrganizacionDetalleDto(
                    organizacion.Id,
                    organizacion.TipoOrganizacionId,
                    organizacion.TipoOrganizacion.Nombre,
                    organizacion.NombreComercial,
                    organizacion.RazonSocial,
                    organizacion.NumeroDocumento,
                    organizacion.Telefono,
                    organizacion.Correo,
                    organizacion.DireccionPrincipal,
                    organizacion.LogoUrl,
                    organizacion.EstaActivo,
                    organizacion.FechaCreacion,
                    organizacion.FechaModificacion,
                    organizacion.Sedes.Count(sede => sede.EstaActivo)))
                .SingleOrDefaultAsync(cancellationToken);

        public Task<Organizacion?> ObtenerParaModificarAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _context.Organizaciones
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(organizacion => organizacion.Id == id, cancellationToken);

        public Task<bool> ExisteDocumentoAsync(
            string numeroDocumento,
            Guid? excluirId = null,
            CancellationToken cancellationToken = default) =>
            _context.Organizaciones
                .IgnoreQueryFilters()
                .AnyAsync(
                    organizacion => organizacion.NumeroDocumento == numeroDocumento &&
                                    (!excluirId.HasValue || organizacion.Id != excluirId.Value),
                    cancellationToken);

        public Task<bool> TipoValidoAsync(
            Guid tipoOrganizacionId,
            CancellationToken cancellationToken = default) =>
            _context.TiposOrganizacion.AnyAsync(
                tipo => tipo.Id == tipoOrganizacionId && tipo.EstaActivo,
                cancellationToken);

        public async Task<IReadOnlyList<TipoOrganizacionOpcionDto>> ListarTiposActivosAsync(
            CancellationToken cancellationToken = default) =>
            await _context.TiposOrganizacion
                .AsNoTracking()
                .Where(tipo => tipo.EstaActivo)
                .OrderBy(tipo => tipo.Nombre)
                .Select(tipo => new TipoOrganizacionOpcionDto(tipo.Id, tipo.Nombre))
                .ToListAsync(cancellationToken);

        public void Agregar(Organizacion organizacion) =>
            _context.Organizaciones.Add(organizacion);

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
                    "La organización entra en conflicto con un registro existente.",
                    exception);
            }
        }
    }
}
