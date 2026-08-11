using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ClienteRepository(ApplicationDbContext context) : IClienteRepository
{
    public async Task<PaginaResultado<ClienteListaDto>> ListarAsync(
        ClienteFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.OrganizacionId == filtro.OrganizacionId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            consulta = consulta.Where(cliente =>
                cliente.NumeroDocumento.Contains(busqueda) ||
                cliente.Nombres.Contains(busqueda) ||
                cliente.Apellidos.Contains(busqueda) ||
                (cliente.Nombres + " " + cliente.Apellidos).Contains(busqueda) ||
                (cliente.Apellidos + " " + cliente.Nombres).Contains(busqueda) ||
                (cliente.Correo != null && cliente.Correo.Contains(busqueda)) ||
                (cliente.Telefono != null && cliente.Telefono.Contains(busqueda)));
        }

        if (filtro.TipoDocumento.HasValue)
            consulta = consulta.Where(cliente => cliente.TipoDocumento == filtro.TipoDocumento.Value);

        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(cliente => cliente.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(cliente => !cliente.EstaActivo),
            _ => consulta
        };

        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .OrderBy(cliente => cliente.Apellidos)
            .ThenBy(cliente => cliente.Nombres)
            .ThenBy(cliente => cliente.Id)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(cliente => new ClienteListaDto(
                cliente.Id,
                cliente.OrganizacionId,
                cliente.TipoDocumento,
                cliente.NumeroDocumento,
                cliente.Nombres,
                cliente.Apellidos,
                cliente.Nombres + " " + cliente.Apellidos,
                cliente.Correo,
                cliente.Telefono,
                cliente.EstaActivo))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<ClienteListaDto>(elementos, pagina, tamano, total);
    }

    public Task<ClienteDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => new ClienteDetalleDto(
                cliente.Id,
                cliente.OrganizacionId,
                cliente.TipoDocumento,
                cliente.NumeroDocumento,
                cliente.Nombres,
                cliente.Apellidos,
                cliente.Nombres + " " + cliente.Apellidos,
                cliente.Correo,
                cliente.Telefono,
                cliente.Direccion,
                cliente.FechaNacimiento,
                cliente.Observaciones,
                cliente.EstaActivo,
                cliente.FechaCreacion,
                cliente.FechaModificacion,
                cliente.CreadoPorUsuarioId,
                cliente.ModificadoPorUsuarioId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Cliente?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Clientes
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(
        Guid organizacionId,
        TipoDocumento tipoDocumento,
        string numeroDocumento,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default) =>
        context.Clientes
            .IgnoreQueryFilters()
            .AnyAsync(cliente =>
                cliente.OrganizacionId == organizacionId &&
                cliente.TipoDocumento == tipoDocumento &&
                cliente.NumeroDocumento == numeroDocumento &&
                (!excluirId.HasValue || cliente.Id != excluirId.Value),
                cancellationToken);

    public void Agregar(Cliente cliente) => context.Clientes.Add(cliente);

    public async Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictoPersistenciaException(
                "El cliente entra en conflicto con un registro existente.", exception);
        }
    }
}
