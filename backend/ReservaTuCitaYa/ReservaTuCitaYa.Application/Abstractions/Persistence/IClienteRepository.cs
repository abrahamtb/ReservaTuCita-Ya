using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IClienteRepository
{
    Task<PaginaResultado<ClienteListaDto>> ListarAsync(
        ClienteFiltroDto filtro,
        CancellationToken cancellationToken = default);

    Task<ClienteDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Cliente?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteDocumentoAsync(
        Guid organizacionId,
        TipoDocumento tipoDocumento,
        string numeroDocumento,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);

    void Agregar(Cliente cliente);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
