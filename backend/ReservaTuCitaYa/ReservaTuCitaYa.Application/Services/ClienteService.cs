using System.Net.Mail;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services;

public sealed class ClienteService(
    IClienteRepository clienteRepository,
    IOrganizacionRepository organizacionRepository) : IClienteService
{
    public const string MensajeDocumentoDuplicado =
        "Ya existe un cliente con el mismo tipo y número de documento en esta organización.";

    public async Task<ResultadoOperacion<PaginaResultado<ClienteListaDto>>> ListarAsync(
        ClienteFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var errorOrganizacion = await ValidarOrganizacionAsync(
            filtro.OrganizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion<PaginaResultado<ClienteListaDto>>.Fallo(
                errorOrganizacion, TipoErrorOperacion.NoEncontrado);

        if (filtro.TipoDocumento.HasValue && !TipoDocumentoValido(filtro.TipoDocumento.Value))
            return ResultadoOperacion<PaginaResultado<ClienteListaDto>>.Fallo(
                "El tipo de documento no es válido.");

        return ResultadoOperacion<PaginaResultado<ClienteListaDto>>.Exito(
            await clienteRepository.ListarAsync(filtro, cancellationToken));
    }

    public async Task<ResultadoOperacion<ClienteDetalleDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var detalle = await clienteRepository.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null
            ? ResultadoOperacion<ClienteDetalleDto>.Fallo(
                "El cliente no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<ClienteDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var error = Validar(solicitud);
        if (error is not null)
            return ResultadoOperacion<Guid>.Fallo(error);

        var errorOrganizacion = await ValidarOrganizacionAsync(
            solicitud.OrganizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion<Guid>.Fallo(
                errorOrganizacion, TipoErrorOperacion.NoEncontrado);

        var numeroDocumento = solicitud.NumeroDocumento.Trim();
        if (await clienteRepository.ExisteDocumentoAsync(
                solicitud.OrganizacionId,
                solicitud.TipoDocumento,
                numeroDocumento,
                cancellationToken: cancellationToken))
            return ResultadoOperacion<Guid>.Fallo(
                MensajeDocumentoDuplicado, TipoErrorOperacion.Conflicto);

        var cliente = new Cliente
        {
            OrganizacionId = solicitud.OrganizacionId,
            TipoDocumento = solicitud.TipoDocumento,
            NumeroDocumento = numeroDocumento,
            Nombres = solicitud.Nombres.Trim(),
            Apellidos = solicitud.Apellidos.Trim(),
            Correo = LimpiarOpcional(solicitud.Correo),
            Telefono = LimpiarOpcional(solicitud.Telefono),
            Direccion = LimpiarOpcional(solicitud.Direccion),
            FechaNacimiento = solicitud.FechaNacimiento,
            Observaciones = LimpiarOpcional(solicitud.Observaciones)
        };
        clienteRepository.Agregar(cliente);

        try
        {
            await clienteRepository.GuardarAsync(cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<Guid>.Fallo(
                MensajeDocumentoDuplicado, TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion<Guid>.Exito(cliente.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.ObtenerParaModificarAsync(
            solicitud.Id, cancellationToken);
        var estado = ValidarRegistro(cliente);
        if (estado is not null)
            return estado;

        var error = Validar(solicitud);
        if (error is not null)
            return ResultadoOperacion.Fallo(error);

        var errorOrganizacion = await ValidarOrganizacionAsync(
            cliente!.OrganizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion.Fallo(
                errorOrganizacion, TipoErrorOperacion.NoEncontrado);

        var numeroDocumento = solicitud.NumeroDocumento.Trim();
        if (await clienteRepository.ExisteDocumentoAsync(
                cliente.OrganizacionId,
                solicitud.TipoDocumento,
                numeroDocumento,
                cliente.Id,
                cancellationToken))
            return ResultadoOperacion.Fallo(
                MensajeDocumentoDuplicado, TipoErrorOperacion.Conflicto);

        cliente.TipoDocumento = solicitud.TipoDocumento;
        cliente.NumeroDocumento = numeroDocumento;
        cliente.Nombres = solicitud.Nombres.Trim();
        cliente.Apellidos = solicitud.Apellidos.Trim();
        cliente.Correo = LimpiarOpcional(solicitud.Correo);
        cliente.Telefono = LimpiarOpcional(solicitud.Telefono);
        cliente.Direccion = LimpiarOpcional(solicitud.Direccion);
        cliente.FechaNacimiento = solicitud.FechaNacimiento;
        cliente.Observaciones = LimpiarOpcional(solicitud.Observaciones);
        cliente.FechaModificacion = DateTime.UtcNow;

        try
        {
            await clienteRepository.GuardarAsync(cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion.Fallo(
                MensajeDocumentoDuplicado, TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id,
        bool estaActivo,
        CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(cliente);
        if (estado is not null)
            return estado;

        cliente!.EstaActivo = estaActivo;
        cliente.FechaModificacion = DateTime.UtcNow;
        await clienteRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(cliente);
        if (estado is not null)
            return estado;

        cliente!.EstaActivo = false;
        cliente.EstaEliminado = true;
        cliente.FechaModificacion = DateTime.UtcNow;
        await clienteRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private async Task<string?> ValidarOrganizacionAsync(
        Guid organizacionId,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(
            organizacionId, cancellationToken);
        return organizacion is null || organizacion.EstaEliminado
            ? "La organización no existe o fue eliminada."
            : null;
    }

    private static ResultadoOperacion? ValidarRegistro(Cliente? cliente)
    {
        if (cliente is null || cliente.EstaEliminado)
            return ResultadoOperacion.Fallo(
                "El cliente no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado);
        return null;
    }

    private static string? Validar(GuardarClienteSolicitud solicitud)
    {
        if (!TipoDocumentoValido(solicitud.TipoDocumento))
            return "El tipo de documento es obligatorio y debe ser válido.";
        if (string.IsNullOrWhiteSpace(solicitud.NumeroDocumento))
            return "El número de documento es obligatorio.";
        if (solicitud.NumeroDocumento.Trim().Length > 20)
            return "El número de documento no puede superar 20 caracteres.";
        if (string.IsNullOrWhiteSpace(solicitud.Nombres))
            return "Los nombres son obligatorios.";
        if (solicitud.Nombres.Trim().Length > 100)
            return "Los nombres no pueden superar 100 caracteres.";
        if (string.IsNullOrWhiteSpace(solicitud.Apellidos))
            return "Los apellidos son obligatorios.";
        if (solicitud.Apellidos.Trim().Length > 100)
            return "Los apellidos no pueden superar 100 caracteres.";
        if (solicitud.Correo?.Trim().Length > 150)
            return "El correo no puede superar 150 caracteres.";
        if (!string.IsNullOrWhiteSpace(solicitud.Correo) &&
            !MailAddress.TryCreate(solicitud.Correo.Trim(), out _))
            return "El correo no tiene un formato válido.";
        if (solicitud.Telefono?.Trim().Length > 30)
            return "El teléfono no puede superar 30 caracteres.";
        if (solicitud.Direccion?.Trim().Length > 250)
            return "La dirección no puede superar 250 caracteres.";
        if (solicitud.FechaNacimiento > DateOnly.FromDateTime(DateTime.UtcNow))
            return "La fecha de nacimiento no puede ser futura.";
        if (solicitud.Observaciones?.Trim().Length > 500)
            return "Las observaciones no pueden superar 500 caracteres.";
        return null;
    }

    private static bool TipoDocumentoValido(TipoDocumento tipoDocumento) =>
        tipoDocumento != TipoDocumento.NoDefinido && Enum.IsDefined(tipoDocumento);

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
